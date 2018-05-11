using System;

namespace Lotech.Data.Generics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class GenericExistsEntity<TEntity> : Operations.Common.CommonExistsEntity<TEntity>
       where TEntity : class
    {
    }
}
