using System;

namespace Lotech.Data.Generics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class GenericLoadEntity<TEntity> : Operations.Common.CommonLoadEntity<TEntity>, IOperationProvider<Func<IDatabase, TEntity, TEntity>>
       where TEntity : class
    {
    }
}
