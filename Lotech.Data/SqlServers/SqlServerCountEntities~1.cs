using Lotech.Data.Operations.Common;
using System;

namespace Lotech.Data.SqlServers
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SqlServerCountEntities<TEntity> : CommonCountEntities<TEntity>, IOperationProvider<Func<IDatabase, int>>
       where TEntity : class
    {
        public SqlServerCountEntities() : base(SqlServerDatabase.Quote) { }
    }
}
