using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Lotech.Data.Descriptors;

namespace Lotech.Data.SqlServers
{
    using Utils;
    using static SqlServerDatabase;
    class BulkInsertOperationBuilder<TEntity> : IOperationProvider<Action<IDatabase, IEnumerable<TEntity>>> where TEntity : class
    {
        public Action<IDatabase, IEnumerable<TEntity>> Create(IEntityDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (descriptor.Type != typeof(TEntity)) throw new InvalidOperationException("实体描述符与当前类型不匹配.");

            var columns = descriptor.Members.Where(_ => !_.DbGenerated)
                    .Select((_, i) => new MemberTuple<TEntity>(
                        _.Name,
                        _.DbType,
                        string.Empty,
                       MemberAccessor<TEntity, object>.GetGetter(_.Member)
                    )).ToArray();

            var destinationTableName = string.IsNullOrEmpty(descriptor.Schema) ? Quote(descriptor.Name)
                        : (Quote(descriptor.Schema) + "." + Quote(descriptor.Name));

            var outputs = descriptor.Members.Where(_ => _.DbGenerated)
                    .Select((_, i) => new MemberTuple<TEntity>(
                        _.Name,
                        _.DbType,
                        string.Empty,
                       MemberAccessor<TEntity, object>.GetGetter(_.Member),
                       MemberAccessor<TEntity, object>.GetSetter(_.Member)
                    )).ToArray();

            if (outputs.Length == 0)
                return (db, entities) =>
                {
                    var sqlserver = db as SqlServerDatabase;
                    if (sqlserver == null) throw new NotSupportedException();

                    var bulkCopy = BulkCopy<TEntity>.Create(sqlserver, Operation.Insert);
                    if (bulkCopy == null) throw new NotSupportedException();

                    bulkCopy.WriteTo(destinationTableName, entities);
                };
            else
            {
                var temporaryTableName = Quote("#BulkInsert/" + descriptor.Name + "/" + Guid.NewGuid().ToString("N")
                                           + "/" + DateTime.Now.Ticks.ToString("x"));
                var columnNames = string.Join(", ", columns.Select(_ => Quote(_.Name)));
                var createTempTableSql = "SELECT TOP 0 * INTO " + temporaryTableName + " FROM " + destinationTableName;
                var insertSql = "INSERT TOP (@count) INTO " + destinationTableName + "(" + columnNames + ")"
                            + "\r\n OUTPUT " + string.Join(", ", outputs.Select(_ => "INSERTED." + Quote(_.Name)))
                            + "\r\n SELECT " + columnNames + " FROM " + temporaryTableName + " OPTION(KEEPFIXED PLAN, OPTIMIZE FOR (@count=0));"
                            + "\r\nDROP TABLE " + temporaryTableName;

                var output = CompileOutputAction(descriptor.Members);

                return (db, entities) =>
                {
                    var sqlserver = db as SqlServerDatabase;
                    if (sqlserver == null) throw new NotSupportedException();

                    var bulkCopy = BulkCopy<TEntity>.Create(sqlserver, Operation.Insert);
                    if (bulkCopy == null) throw new NotSupportedException();

                    var entityList = (entities as IList<TEntity> ?? entities.ToArray());
                    if (entityList.Count == 0) return;

                    using (var transaction = new TransactionManager())
                    {
                        db.ExecuteNonQuery(createTempTableSql);

                        bulkCopy.WriteTo(temporaryTableName, entityList);

                        using (var command = db.GetSqlStringCommand(insertSql))
                        {
                            db.AddInParameter(command, "@count", System.Data.DbType.Int64, entityList.Count);

                            using (var reader = db.ExecuteEntityReader<TEntity>(command))// 执行并回写输出字段
                            {
                                for (var index = 0; reader.Read(); index++)
                                    output(entityList[index], reader.GetValue());
                            }
                        }
                        transaction.Commit();
                    }
                };
            }
        }

        static Action<TEntity, TEntity> CompileOutputAction(IList<IMemberDescriptor> members)
        {
            var entity = Expression.Parameter(typeof(TEntity), "entity");
            var value = Expression.Parameter(typeof(TEntity), "value");

            return Expression.Lambda<Action<TEntity, TEntity>>(
                    Expression.Block(members.Where(_ => _.DbGenerated).Select(_ => Expression.Assign(
                            Expression.MakeMemberAccess(entity, _.Member),
                                Expression.MakeMemberAccess(value, _.Member))
                        ))
                    , entity, value
                ).Compile();
        }
    }
}
