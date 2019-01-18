using System;
using System.Collections.Generic;
using System.Linq;
using Lotech.Data.Descriptors;

namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;
    class BulkDeleteOperationBuilder<TEntity> : IOperationProvider<Action<IDatabase, IEnumerable<TEntity>>> where TEntity : class
    {
        public Action<IDatabase, IEnumerable<TEntity>> Create(IEntityDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (descriptor.Type != typeof(TEntity)) throw new InvalidOperationException("实体描述符与当前类型不匹配.");
            if (descriptor.Keys == null || descriptor.Keys.Length == 0)
                throw new InvalidOperationException("仅支持具备主键数据表的删除操作.");

            var join = string.Join(" AND "
                , descriptor.Keys.Select(_ => "t." + Quote(_.Name) + " = s." + Quote(_.Name)));

            var destinationTableName = string.IsNullOrEmpty(descriptor.Schema) ? Quote(descriptor.Name)
                        : (Quote(descriptor.Schema) + "." + Quote(descriptor.Name));

            var temporaryTableName = Quote("#BulkDelete/" + descriptor.Name + "/" + Guid.NewGuid().ToString("N")
                                        + "/" + DateTime.Now.Ticks.ToString("x"));
            var createtemporarySql = "SELECT TOP 0 * INTO " + temporaryTableName + " FROM " + destinationTableName;
            var deleteSql = "DELETE t FROM " + destinationTableName + " t JOIN " + temporaryTableName + " s ON " + join
                            + ";\r\nDROP TABLE" + temporaryTableName;
            var keys = descriptor.Keys.Select(_ => _.Name).ToArray();
            Func<MemberTuple<TEntity>, bool> keyFilter = column => Array.IndexOf(keys, column.Name) != -1;

            return (db, entities) =>
            {
                var sqlserver = db as SqlServerDatabase;
                if (sqlserver == null) throw new NotSupportedException();

                var bulkCopy = BulkCopy<TEntity>.Create(sqlserver, Operation.Delete);
                if (bulkCopy == null) throw new NotSupportedException();

                using (var transaction = new TransactionManager())
                {
                    db.ExecuteNonQuery(createtemporarySql);
                    bulkCopy.WriteTo(temporaryTableName, entities, keyFilter);
                    db.ExecuteNonQuery(deleteSql);

                    transaction.Commit();
                }
            };
        }
    }
}
