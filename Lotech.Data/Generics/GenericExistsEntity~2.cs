using System;

namespace Lotech.Data.Generics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class GenericExistsEntity<TEntity, TKey> : Operations.Common.CommonExistsEntity<TEntity, TKey>
       where TEntity : class
    {
    }
}
