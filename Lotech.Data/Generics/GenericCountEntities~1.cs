using Lotech.Data.Operations.Common;
using System;

namespace Lotech.Data.Generics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class GenericCountEntities<TEntity> : CommonCountEntities<TEntity>, IOperationProvider<Func<IDatabase, int>>
       where TEntity : class
    {
    }
}
