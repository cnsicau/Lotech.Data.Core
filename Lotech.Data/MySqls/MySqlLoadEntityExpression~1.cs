using System;
using System.Linq.Expressions;

namespace Lotech.Data.MySqls
{
    using static MySqlDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class MySqlLoadEntityExpression<TEntity> : Operations.Common.CommonLoadEntityExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity>>
        where TEntity : class
    {
        public MySqlLoadEntityExpression() : base(
            db => new MySqlExpressionVisitor<TEntity>(db),
            Quote
        )
        { }
    }
}
