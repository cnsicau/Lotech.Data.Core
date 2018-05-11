namespace Lotech.Data.Generics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class GenericLoadEntity<TEntity, TKey> : Operations.Common.CommonLoadEntity<TEntity, TKey>
       where TEntity : class
    {
    }
}
