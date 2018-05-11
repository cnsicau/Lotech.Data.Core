using System;
using System.Linq.Expressions;

namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SqlServerDeleteEntitiesExpression<TEntity> : Operations.Common.CommonDeleteEntitiesExpression<TEntity>, IOperationProvider<Action<IDatabase, Expression<Func<TEntity, bool>>>>
        where TEntity : class
    {
        public SqlServerDeleteEntitiesExpression() : base(
            db => new SqlServerExpressionVisitor<TEntity>(db),
            Quote
        )
        { }
    }
}
