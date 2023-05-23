using Lotech.Data.Operations.Common;
using System;

namespace Lotech.Data.MySqls
{
    using static MySqlEntityServices;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class MySqlFindEntities<TEntity> : CommonFindEntities<TEntity>, IOperationProvider<Func<IDatabase, TEntity[]>>
       where TEntity : class
    {
        public MySqlFindEntities() : base(Quote) { }
    }
}
