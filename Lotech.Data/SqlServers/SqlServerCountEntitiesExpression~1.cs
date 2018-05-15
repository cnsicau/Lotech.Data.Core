using System;
using System.Linq.Expressions;

namespace Lotech.Data.SqlServers
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SqlServerCountEntitiesExpression<TEntity> : Operations.Common.CommonCountEntitiesExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, int>>
        where TEntity : class
    {
        public SqlServerCountEntitiesExpression() : base(_ => new SqlServerExpressionVisitor<TEntity>(_), SqlServerDatabase.Quote) { }
    }
}
