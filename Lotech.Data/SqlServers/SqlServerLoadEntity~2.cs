using System;

namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class SqlServerLoadEntity<TEntity, TKey> : Operations.Common.CommonLoadEntity<TEntity, TKey>, IOperationProvider<Func<IDatabase, TKey, TEntity>>
        where TEntity : class
    {
        public SqlServerLoadEntity() : base(Quote, BuildParameter) { }
    }
}
