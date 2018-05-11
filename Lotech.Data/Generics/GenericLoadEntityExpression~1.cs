using System;
using System.Linq.Expressions;

namespace Lotech.Data.Generics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class GenericLoadEntityExpression<TEntity> : Operations.Common.CommonLoadEntityExpression<TEntity>, IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity>>
       where TEntity : class
    {
    }
}
