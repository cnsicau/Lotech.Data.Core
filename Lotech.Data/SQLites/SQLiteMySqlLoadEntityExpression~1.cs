using System;
using System.Linq.Expressions;

namespace Lotech.Data.SQLites
{
    using static SQLiteDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SQLiteLoadEntityExpression<TEntity> : Operations.Common.CommonLoadEntityExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity>>
        where TEntity : class
    {
        public SQLiteLoadEntityExpression() : base(
            db => new SQLiteExpressionVisitor<TEntity>(db),
            Quote
        )
        { }
    }
}
