using System;
using System.Linq.Expressions;

namespace Lotech.Data.MySqls
{
    using static MySqlEntityServices;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class MySqlFindEntitiesExpression<TEntity> : Operations.Common.CommonFindEntitiesExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity[]>>
       where TEntity : class
    {
        public MySqlFindEntitiesExpression() : base(
            db => new MySqlExpressionVisitor<TEntity>(db, Descriptors.Operation.Select),
            Quote
        )
        { }
    }
}
