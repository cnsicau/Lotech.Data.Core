using System;
using System.Linq.Expressions;

namespace Lotech.Data.MySqls
{
    using static MySqlDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class MySqlDeleteEntitiesExpression<TEntity> : Operations.Common.CommonDeleteEntitiesExpression<TEntity>, IOperationProvider<Action<IDatabase, Expression<Func<TEntity, bool>>>>
        where TEntity : class
    {
        public MySqlDeleteEntitiesExpression() : base(
            db => new MySqlExpressionVisitor<TEntity>(db),
            Quote
        )
        { }
    }
}
