using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lotech.Data.Descriptors;
using Lotech.Data.Utils;

namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;
    class BulkUpdateOperationBuilder<TEntity> : IOperationProvider<Action<IDatabase, IEnumerable<TEntity>>> where TEntity : class
    {
        public Action<IDatabase, IEnumerable<TEntity>> Create(IEntityDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (descriptor.Type != typeof(TEntity)) throw new InvalidOperationException("实体描述符与当前类型不匹配.");
            if (descriptor.Keys == null || descriptor.Keys.Length == 0)
                throw new InvalidOperationException("仅支持具备主键数据表的更新操作.");

            var join = string.Join(" AND "
                , descriptor.Keys.Select(_ => "t." + Quote(_.Name) + " = s." + Quote(_.Name)));
            var set = string.Join(", ", descriptor.Members.Except(descriptor.Keys).Where(_ => !_.DbGenerated)
                    .Select(_ => Quote(_.Name) + " = s." + Quote(_.Name)));

            if (string.IsNullOrEmpty(set)) throw new InvalidOperationException("未找到需要更新的列.");

            var destinationTableName = string.IsNullOrEmpty(descriptor.Schema) ? Quote(descriptor.Name)
                        : (Quote(descriptor.Schema) + "." + Quote(descriptor.Name));

            var temporaryTableName = Quote("#BulkUpdate/" + descriptor.Name + "/" + Guid.NewGuid().ToString("N")
                                        + "/" + DateTime.Now.Ticks.ToString("x"));
            var createTempTable = "SELECT TOP 0 * INTO " + temporaryTableName + " FROM " + destinationTableName;

            var outputColumns = descriptor.Members.Where(_ => _.DbGenerated).ToArray();

            var updateSql = "UPDATE t SET " + set
                        + (outputColumns.Length > 0 ? " OUTPUT " + string.Join(", ", descriptor.Keys.Concat(outputColumns).Select(_ => "INSERTED." + Quote(_.Name))) : null)
                        + " FROM " + destinationTableName + " t JOIN " + temporaryTableName + " s ON " + join
                        + ";\r\nDROP TABLE " + temporaryTableName;

            Action<IDatabase, BulkCopy<TEntity>, IEnumerable<TEntity>> executeUpdate;

            if (outputColumns.Length == 0)
                executeUpdate = (db, bulkCopy, entities) =>
                {
                    bulkCopy.WriteTo(temporaryTableName, entities);
                    db.ExecuteNonQuery(updateSql);
                };
            else
            {
                var hash = MemberAccessor.CreateHashKey<TEntity>(descriptor.Keys);
                var outputAssign = MemberAccessor.CreateAssign<TEntity>(descriptor.Members.Where(_ => _.DbGenerated).Select(_ => _.Member));

                executeUpdate = (db, bulkCopy, entities) =>
                {
                    var source = (entities as IList<TEntity>) ?? entities.ToArray();
                    bulkCopy.WriteTo(temporaryTableName, source);

                    using (var entityReader = db.ExecuteEntityReader<TEntity>(updateSql).GetEnumerator())
                    {
                        var dictionary = new Dictionary<IStructuralEquatable, TEntity>(source.Count + 8);
                        while (entityReader.MoveNext())
                        {
                            var value = entityReader.Current;
                            dictionary.Add(hash(value), value);
                        }

                        foreach (var entity in source)
                        {
                            TEntity value;
                            if (dictionary.TryGetValue(hash(entity), out value))
                            {
                                outputAssign(entity, value);
                            }
                        }
                    }
                };
            }

            return (db, entities) =>
            {
                var sqlserver = db as SqlServerDatabase;
                if (sqlserver == null) throw new NotSupportedException();

                var bulkCopy = BulkCopy<TEntity>.Create(sqlserver, Operation.Update);
                if (bulkCopy == null) throw new NotSupportedException();

                using (var transaction = new TransactionManager())
                {
                    db.ExecuteNonQuery(createTempTable);

                    executeUpdate(db, bulkCopy, entities);

                    transaction.Commit();
                }
            };
        }
    }
}
