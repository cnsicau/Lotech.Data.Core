using Lotech.Data.Operations.Common;
using System;

namespace Lotech.Data.MySqls
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class MySqlCountEntities<TEntity> : CommonCountEntities<TEntity>, IOperationProvider<Func<IDatabase, int>>
       where TEntity : class
    {
        public MySqlCountEntities() : base(MySqlDatabase.Quote) { }
    }
}
