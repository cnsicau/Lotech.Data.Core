using System;

namespace Lotech.Data.MySqls
{
    using static MySqlDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class MySqlExistsEntity<TEntity, TKey> : Operations.Common.CommonExistsEntity<TEntity, TKey>, IOperationProvider<Func<IDatabase, TKey, bool>>
       where TEntity : class
    {
        public MySqlExistsEntity() : base(Quote, BuildParameter) { }
    }
}
