using System;

namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SqlServerLoadEntity<TEntity> : Operations.Common.CommonLoadEntity<TEntity>, IOperationProvider<Func<IDatabase, TEntity, TEntity>>
        where TEntity : class
    {
        public SqlServerLoadEntity() : base(Quote, BuildParameter) { }
    }
}
