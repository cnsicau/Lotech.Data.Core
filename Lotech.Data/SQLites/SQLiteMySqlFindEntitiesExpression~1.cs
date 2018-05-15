using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lotech.Data.SQLites
{
    using static SQLiteDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SQLiteFindEntitiesExpression<TEntity> : Operations.Common.CommonFindEntitiesExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity[]>>
       where TEntity : class
    {
        public SQLiteFindEntitiesExpression() : base(
            db => new SQLiteExpressionVisitor<TEntity>(db),
            Quote
        )
        { }
    }
}
