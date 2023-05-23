using Lotech.Data.Operations.Common;
using System;

namespace Lotech.Data.SQLites
{
    using static SQLiteEntityServices;
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
