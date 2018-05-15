using System;
using System.Linq.Expressions;

namespace Lotech.Data.Oracles
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class OracleCountEntitiesExpression<TEntity> : Operations.Common.CommonCountEntitiesExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, int>>
        where TEntity : class
    {
        public OracleCountEntitiesExpression() : base(_ => new OracleExpressionVisitor<TEntity>(_), OracleDatabase.Quote) { }
    }
}
