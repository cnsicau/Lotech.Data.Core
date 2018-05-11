using System;
using System.Linq.Expressions;

namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SqlServerLoadEntityExpression<TEntity> : Operations.Common.CommonLoadEntityExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity>>
       where TEntity : class
    {

        public SqlServerLoadEntityExpression() : base(
            db => new SqlServerExpressionVisitor<TEntity>(db),
            Quote
        )
        { }
    }
}
