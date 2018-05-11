using System;
using System.Linq.Expressions;

namespace Lotech.Data.Oracles
{
    using static OracleDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class OracleDeleteEntitiesExpression<TEntity> : Operations.Common.CommonDeleteEntitiesExpression<TEntity>, IOperationProvider<Action<IDatabase, Expression<Func<TEntity, bool>>>>
        where TEntity : class
    {
        public OracleDeleteEntitiesExpression() : base(
            db => new OracleExpressionVisitor<TEntity>(db),
            Quote
        )
        { }
    }
}
