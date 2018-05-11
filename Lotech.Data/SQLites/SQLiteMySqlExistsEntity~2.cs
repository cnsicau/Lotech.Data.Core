using System;

namespace Lotech.Data.SQLites
{
    using static SQLiteDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class SQLiteExistsEntity<TEntity, TKey> : Operations.Common.CommonExistsEntity<TEntity, TKey>, IOperationProvider<Func<IDatabase, TKey, bool>>
       where TEntity : class
    {
        public SQLiteExistsEntity() : base(Quote, BuildParameter) { }
    }
}
