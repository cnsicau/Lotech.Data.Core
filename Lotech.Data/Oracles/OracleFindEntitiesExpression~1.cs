using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lotech.Data.Oracles
{
    using static OracleDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class OracleFindEntitiesExpression<TEntity> : Operations.Common.CommonFindEntitiesExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, IEnumerable<TEntity>>>
       where TEntity : class
    {
        public OracleFindEntitiesExpression() : base(
            db => new OracleExpressionVisitor<TEntity>(db),
            Quote
        )
        { }
    }
}
