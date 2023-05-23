using Lotech.Data.Operations.Common;
using System;

namespace Lotech.Data.Oracles
{
    using static OracleDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class OracleFindEntities<TEntity> : CommonFindEntities<TEntity>, IOperationProvider<Func<IDatabase, TEntity[]>>
       where TEntity : class
    {
        public OracleFindEntities() : base(Quote) { }
    }
}
