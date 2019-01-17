#if NET_4
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Lotech.Data.Descriptors;

namespace Lotech.Data.SqlServers
{
    using Utils;
    using static SqlServerDatabase;
    class BulkCopyOperationBuilder<TEntity> : IOperationProvider<Action<IDatabase, IEnumerable<TEntity>>> where TEntity : class
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

            return (db, entities) =>
            {
                var sqlserver = db as SqlServerDatabase;
                if (sqlserver == null) throw new NotSupportedException();

                const SqlBulkCopyOptions options = SqlBulkCopyOptions.CheckConstraints
                                                    | SqlBulkCopyOptions.FireTriggers
                                                    | SqlBulkCopyOptions.KeepIdentity
                                                    | SqlBulkCopyOptions.KeepNulls;

                using (var command = db.GetSqlStringCommand("SELECT 1"))
                {
                    sqlserver.GetConnection(command);
                    var connection = command.Connection as SqlConnection;

                    var bulkCopy = connection == null
                        ? new SqlBulkCopy(db.ConnectionString, options | SqlBulkCopyOptions.UseInternalTransaction)
                        : command.Transaction is SqlTransaction
                        ? new SqlBulkCopy(connection, options, (SqlTransaction)command.Transaction)
                        : new SqlBulkCopy(db.ConnectionString, options);
                    bulkCopy.DestinationTableName = destinationTableName;
                    for (int i = 0; i < columns.Length; i++)
                    {
                        bulkCopy.ColumnMappings.Add(i, columns[i].Name);
                    }
                    using (var reader = new BulkCopyDataReader<TEntity>(entities, columns))
                    {
                        bulkCopy.WriteToServer(reader);
                    }
                }
            };
        }
    }
}

#endif