using System;
using System.Linq.Expressions;

namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SqlServerExistsEntityExpression<TEntity> : Operations.Common.CommonExistsEntityExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, bool>>
       where TEntity : class
    {
        public SqlServerExistsEntityExpression() : base(_ => new SqlServerExpressionVisitor<TEntity>(_), Quote) { }
    }
}
