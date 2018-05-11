using System;

namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class SqlServerExistsEntity<TEntity, TKey> : Operations.Common.CommonExistsEntity<TEntity, TKey>, IOperationProvider<Func<IDatabase, TKey, bool>>
       where TEntity : class
    {
        public SqlServerExistsEntity() : base(Quote, BuildParameter) { }
    }
}
