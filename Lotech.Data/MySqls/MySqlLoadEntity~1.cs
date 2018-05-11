using System;

namespace Lotech.Data.MySqls
{
    using static MySqlDatabase;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class MySqlLoadEntity<TEntity> : Operations.Common.CommonLoadEntity<TEntity>, IOperationProvider<Func<IDatabase, TEntity, TEntity>>
        where TEntity : class
    {
        public MySqlLoadEntity() : base(Quote, BuildParameter) { }
    }
}
