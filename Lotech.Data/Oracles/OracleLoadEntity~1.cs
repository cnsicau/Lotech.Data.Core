using System;

namespace Lotech.Data.Oracles
{
    using static OracleDatabase;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class OracleLoadEntity<TEntity> : Operations.Common.CommonLoadEntity<TEntity>, IOperationProvider<Func<IDatabase, TEntity, TEntity>>
        where TEntity : class
    {
        public OracleLoadEntity() : base(Quote, BuildParameter) { }
    }
}
