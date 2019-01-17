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

            return (db, entities) =>
            {
                var sqlserver = db as SqlServerDatabase;
                if (sqlserver == null) throw new NotSupportedException();

                var bulkCopy = BulkCopy<TEntity>.Create(sqlserver, Operation.Insert);
                if (bulkCopy == null) throw new NotSupportedException();

                bulkCopy.WriteTo(destinationTableName, entities);
            };
        }
    }
}
