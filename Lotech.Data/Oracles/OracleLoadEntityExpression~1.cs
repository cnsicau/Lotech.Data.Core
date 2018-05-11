using System;
using System.Linq.Expressions;

namespace Lotech.Data.Oracles
{
    using static OracleDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class OracleLoadEntityExpression<TEntity> : Operations.Common.CommonLoadEntityExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity>>
        where TEntity : class
    {

        public OracleLoadEntityExpression() : base(
            db => new OracleExpressionVisitor<TEntity>(db),
            Quote
        )
        { }
    }
}
