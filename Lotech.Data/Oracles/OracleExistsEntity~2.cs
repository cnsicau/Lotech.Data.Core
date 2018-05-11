using System;

namespace Lotech.Data.Oracles
{
    using static OracleDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class OracleExistsEntity<TEntity, TKey> : Operations.Common.CommonExistsEntity<TEntity, TKey>, IOperationProvider<Func<IDatabase, TKey, bool>>
       where TEntity : class
    {
        public OracleExistsEntity() : base(Quote, BuildParameter) { }
    }
}
