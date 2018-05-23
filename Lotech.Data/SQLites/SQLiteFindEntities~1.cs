using Lotech.Data.Operations.Common;
using System;
using System.Collections.Generic;

namespace Lotech.Data.SQLites
{
    using static SQLiteDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SQLiteFindEntities<TEntity> : CommonFindEntities<TEntity>, IOperationProvider<Func<IDatabase, TEntity[]>>
       where TEntity : class
    {
        public SQLiteFindEntities() : base(Quote) { }
    }
}
