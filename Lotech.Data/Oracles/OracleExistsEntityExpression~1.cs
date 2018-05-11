using System;
using System.Linq.Expressions;

namespace Lotech.Data.Oracles
{
    using static OracleDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class OracleExistsEntityExpression<TEntity> : Operations.Common.CommonExistsEntityExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, bool>>
       where TEntity : class
    {
        public OracleExistsEntityExpression() : base(_ => new OracleExpressionVisitor<TEntity>(_), Quote) { }
    }
}
