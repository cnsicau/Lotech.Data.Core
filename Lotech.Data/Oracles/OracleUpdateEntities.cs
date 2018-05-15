using Lotech.Data.Operations.Common;

namespace Lotech.Data.Oracles
{
    class OracleUpdateEntities<TEntity, TSet> : CommonUpdateEntities<TEntity, TSet> where TEntity : class where TSet : class
    {
        public OracleUpdateEntities() : base(OracleDatabase.Quote, OracleDatabase.BuildParameter, _ => new OracleExpressionVisitor<TEntity>(_)) { }
    }
}
