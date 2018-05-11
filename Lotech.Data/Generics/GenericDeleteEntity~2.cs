using System;

namespace Lotech.Data.Generics
{
    /// <summary>
    /// 按主键删除
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class GenericDeleteEntity<TEntity, TKey> : Operations.Common.CommonDeleteEntity<TEntity, TKey>, IOperationProvider<Action<IDatabase, TKey>>
      where TEntity : class
    { }
}
