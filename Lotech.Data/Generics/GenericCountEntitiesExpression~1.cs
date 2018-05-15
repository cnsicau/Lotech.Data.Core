using System;
using System.Linq.Expressions;

namespace Lotech.Data.Generics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class GenericCountEntitiesExpression<TEntity> : Operations.Common.CommonCountEntitiesExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, int>>
        where TEntity : class
    {
    }
}
