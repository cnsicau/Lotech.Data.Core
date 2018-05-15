using System;
using System.Linq.Expressions;

namespace Lotech.Data.MySqls
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class MySqlCountEntitiesExpression<TEntity> : Operations.Common.CommonCountEntitiesExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, int>>
        where TEntity : class
    {
        public MySqlCountEntitiesExpression() : base(_ => new MySqlExpressionVisitor<TEntity>(_), MySqlDatabase.Quote) { }
    }
}
