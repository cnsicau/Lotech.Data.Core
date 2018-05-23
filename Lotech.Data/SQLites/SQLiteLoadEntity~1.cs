using System;

namespace Lotech.Data.SQLites
{
    using static SQLiteDatabase;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SQLiteLoadEntity<TEntity> : Operations.Common.CommonLoadEntity<TEntity>, IOperationProvider<Func<IDatabase, TEntity, TEntity>>
        where TEntity : class
    {
        public SQLiteLoadEntity() : base(Quote, BuildParameter) { }
    }
}
