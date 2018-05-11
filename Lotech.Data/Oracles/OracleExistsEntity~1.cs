using System;

namespace Lotech.Data.Oracles
{
    using static OracleDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class OracleExistsEntity<TEntity> : Operations.Common.CommonExistsEntity<TEntity>, IOperationProvider<Func<IDatabase, TEntity, bool>>
       where TEntity : class
    {
        public OracleExistsEntity() : base(Quote, BuildParameter) { }
    }
}
