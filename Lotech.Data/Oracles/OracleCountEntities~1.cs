using Lotech.Data.Operations.Common;
using System;

namespace Lotech.Data.Oracles
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class OracleCountEntities<TEntity> : CommonCountEntities<TEntity>, IOperationProvider<Func<IDatabase, int>>
       where TEntity : class
    {
        public OracleCountEntities() : base(OracleDatabase.Quote) { }
    }
}
