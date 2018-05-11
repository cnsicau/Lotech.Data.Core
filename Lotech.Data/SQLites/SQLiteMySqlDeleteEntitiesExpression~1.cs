using System;
using System.Linq.Expressions;

namespace Lotech.Data.SQLites
{
    using static SQLiteDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SQLiteDeleteEntitiesExpression<TEntity> : Operations.Common.CommonDeleteEntitiesExpression<TEntity>, IOperationProvider<Action<IDatabase, Expression<Func<TEntity, bool>>>>
        where TEntity : class
    {
        public SQLiteDeleteEntitiesExpression() : base(
            db => new SQLiteExpressionVisitor<TEntity>(db),
            Quote
        )
        { }
    }
}
