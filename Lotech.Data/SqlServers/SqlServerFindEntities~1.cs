using Lotech.Data.Operations.Common;
using System;

namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SqlServerFindEntities<TEntity> : CommonFindEntities<TEntity>, IOperationProvider<Func<IDatabase, TEntity[]>>
       where TEntity : class
    {
        public SqlServerFindEntities() : base(Quote) { }
    }
}
