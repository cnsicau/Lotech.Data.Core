﻿using System;
using System.Linq.Expressions;

namespace Lotech.Data.Generics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class GenericFindEntitiesExpression<TEntity> : Operations.Common.CommonFindEntitiesExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity[]>>
        where TEntity : class
    {
    }
}
