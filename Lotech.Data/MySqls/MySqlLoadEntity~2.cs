using System;

namespace Lotech.Data.MySqls
{
    using static MySqlDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class MySqlLoadEntity<TEntity, TKey> : Operations.Common.CommonLoadEntity<TEntity, TKey>, IOperationProvider<Func<IDatabase, TKey, TEntity>>
       where TEntity : class
    {
        public MySqlLoadEntity() : base(Quote, BuildParameter) { }
    }
}
