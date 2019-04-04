using System;
using System.Collections.Generic;
using System.Linq;
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

            var outputColumns = descriptor.Members.Where(_ => _.DbGenerated).Select(_ => _.Name).ToArray();

            Action<IDatabase, BulkCopy<TEntity>, IEnumerable<TEntity>> execute;

            if (outputColumns.Length == 0)
                execute = (db, bulkCopy, entities) => bulkCopy.WriteTo(destinationTableName, entities);
            else
            {
                var temporaryTableName = Quote("#BulkInsert/" + descriptor.Name + "/" + Guid.NewGuid().ToString("N")
                                           + "/" + DateTime.Now.Ticks.ToString("x"));
                var columnNames = string.Join(", ", columns.Select(_ => Quote(_.Name)));
                var createTempTableSql = "SELECT TOP 0 * INTO " + temporaryTableName + " FROM " + destinationTableName;
                var insertSql = "INSERT TOP (@count) INTO " + destinationTableName + "(" + columnNames + ")"
                            + "\r\n OUTPUT " + string.Join(", ", outputColumns.Select(_ => "INSERTED." + Quote(_)))
                            + "\r\n SELECT " + columnNames + " FROM " + temporaryTableName + " OPTION(KEEPFIXED PLAN, OPTIMIZE FOR (@count=1));"
                            + "\r\nDROP TABLE " + temporaryTableName;

                var outputAssign = MemberAccessor.CreateAssign<TEntity>(descriptor.Members.Where(_ => _.DbGenerated).Select(_ => _.Member));

                execute = (db, bulkCopy, entities) =>
                {
                    var entityList = (entities as IList<TEntity> ?? entities.ToArray());
                    if (entityList.Count == 0) return;

                    using (var transaction = new TransactionManager())
                    {
                        db.ExecuteNonQuery(createTempTableSql);

                        bulkCopy.WriteTo(temporaryTableName, entityList);

                        using (var command = db.GetSqlStringCommand(insertSql))
                        {
                            db.AddInParameter(command, "@count", System.Data.DbType.Int64, entityList.Count);

                            using (var reader = db.ExecuteEntityReader<TEntity>(command).GetEnumerator())// 执行并回写输出字段
                            {
                                for (var index = 0; reader.MoveNext(); index++)
                                    outputAssign(entityList[index], reader.Current);
                            }
                        }
                        transaction.Commit();
                    }
                };
            }
            return (db, entities) =>
            {
                var sqlserver = db as SqlServerDatabase;
                if (sqlserver == null) throw new NotSupportedException();

                var bulkCopy = BulkCopy<TEntity>.Create(sqlserver, Operation.Insert);
                if (bulkCopy == null) throw new NotSupportedException();

                execute(db, bulkCopy, entities);
            };
        }
    }
}
