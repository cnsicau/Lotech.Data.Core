using System;

namespace Lotech.Data.SQLites
{
    using static SQLiteDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class SQLiteLoadEntity<TEntity, TKey> : Operations.Common.CommonLoadEntity<TEntity, TKey>, IOperationProvider<Func<IDatabase, TKey, TEntity>>
       where TEntity : class
    {
        public SQLiteLoadEntity() : base(Quote, BuildParameter) { }
    }
}
