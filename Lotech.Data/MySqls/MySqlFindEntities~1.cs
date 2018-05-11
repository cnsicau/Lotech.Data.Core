using Lotech.Data.Operations.Common;
using System;
using System.Collections.Generic;

namespace Lotech.Data.MySqls
{
    using static MySqlDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class MySqlFindEntities<TEntity> : CommonFindEntities<TEntity>, IOperationProvider<Func<IDatabase, IEnumerable<TEntity>>>
       where TEntity : class
    {
        public MySqlFindEntities() : base(Quote) { }
    }
}
