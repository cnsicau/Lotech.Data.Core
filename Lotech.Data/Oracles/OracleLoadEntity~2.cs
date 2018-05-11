using System;

namespace Lotech.Data.Oracles
{
    using static OracleDatabase;

    class OracleLoadEntity<TEntity, TKey> : Operations.Common.CommonLoadEntity<TEntity, TKey>, IOperationProvider<Func<IDatabase, TKey, TEntity>>
       where TEntity : class
    {
        public OracleLoadEntity() : base(Quote, BuildParameter) { }
    }
}
