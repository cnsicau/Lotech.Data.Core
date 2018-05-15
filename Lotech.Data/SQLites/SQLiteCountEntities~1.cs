using Lotech.Data.Operations.Common;
using System;

namespace Lotech.Data.SQLites
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SQLiteCountEntities<TEntity> : CommonCountEntities<TEntity>, IOperationProvider<Func<IDatabase, int>>
       where TEntity : class
    {
        public SQLiteCountEntities() : base(SQLiteDatabase.Quote) { }
    }
}
