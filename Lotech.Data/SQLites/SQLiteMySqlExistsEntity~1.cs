using System;

namespace Lotech.Data.SQLites
{
    using static SQLiteDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SQLiteExistsEntity<TEntity> : Operations.Common.CommonExistsEntity<TEntity>, IOperationProvider<Func<IDatabase, TEntity, bool>>
       where TEntity : class
    {
        public SQLiteExistsEntity() : base(Quote, BuildParameter) { }
    }
}
