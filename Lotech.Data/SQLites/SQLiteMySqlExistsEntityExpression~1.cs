using System;
using System.Linq.Expressions;

namespace Lotech.Data.SQLites
{
    using static SQLiteDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SQLiteExistsEntityExpression<TEntity> : Operations.Common.CommonExistsEntityExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, bool>>
       where TEntity : class
    {
        public SQLiteExistsEntityExpression() : base(_ => new SQLiteExpressionVisitor<TEntity>(_), Quote) { }
    }
}
