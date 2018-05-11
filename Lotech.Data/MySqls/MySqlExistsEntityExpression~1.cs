using System;
using System.Linq.Expressions;

namespace Lotech.Data.MySqls
{
    using static MySqlDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class MySqlExistsEntityExpression<TEntity> : Operations.Common.CommonExistsEntityExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, bool>>
       where TEntity : class
    {
        public MySqlExistsEntityExpression() : base(_ => new MySqlExpressionVisitor<TEntity>(_), Quote) { }
    }
}
