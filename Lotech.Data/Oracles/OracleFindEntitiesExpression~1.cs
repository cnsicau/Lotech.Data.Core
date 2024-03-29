﻿using System;
using System.Linq.Expressions;

namespace Lotech.Data.Oracles
{
    using static OracleDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class OracleFindEntitiesExpression<TEntity> : Operations.Common.CommonFindEntitiesExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity[]>>
       where TEntity : class
    {
        public OracleFindEntitiesExpression() : base(
            db => new OracleExpressionVisitor<TEntity>(db, Descriptors.Operation.Select),
            Quote
        )
        { }
    }
}
