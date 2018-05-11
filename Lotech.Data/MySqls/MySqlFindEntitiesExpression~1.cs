using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lotech.Data.MySqls
{
    using static MySqlDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class MySqlFindEntitiesExpression<TEntity> : Operations.Common.CommonFindEntitiesExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, IEnumerable<TEntity>>>
       where TEntity : class
    {
        public MySqlFindEntitiesExpression() : base(
            db => new MySqlExpressionVisitor<TEntity>(db),
            Quote
        )
        { }
    }
}
