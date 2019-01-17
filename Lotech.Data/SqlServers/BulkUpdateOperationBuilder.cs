using System;
using System.Collections.Generic;
using System.Linq;
using Lotech.Data.Descriptors;

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
            var updateSql = "UPDATE t SET " + set + " FROM " + destinationTableName + " t JOIN " + temporaryTableName + " s ON " + join;

            return (db, entities) =>
            {
                var sqlserver = db as SqlServerDatabase;
                if (sqlserver == null) throw new NotSupportedException();

                var bulkCopy = BulkCopy<TEntity>.Create(sqlserver, Operation.Update);
                if (bulkCopy == null) throw new NotSupportedException();

                using (var transaction = new TransactionManager())
                {
                    db.ExecuteNonQuery(createTempTable);
                    bulkCopy.WriteTo(temporaryTableName, entities);
                    db.ExecuteNonQuery(updateSql);

                    transaction.Commit();
                }
            };
        }
    }
}
