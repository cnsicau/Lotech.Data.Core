using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SqlServerFindEntitiesExpression<TEntity> : Operations.Common.CommonFindEntitiesExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity[]>>
       where TEntity : class
    {
        public SqlServerFindEntitiesExpression() : base(
            db => new SqlServerExpressionVisitor<TEntity>(db),
            Quote
        )
        { }
    }
}
