using System;
using System.Linq.Expressions;

namespace Lotech.Data.SQLites
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SQLiteCountEntitiesExpression<TEntity> : Operations.Common.CommonCountEntitiesExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, int>>
        where TEntity : class
    {
        public SQLiteCountEntitiesExpression() : base(_ => new SQLiteExpressionVisitor<TEntity>(_), SQLiteDatabase.Quote) { }
    }
}
