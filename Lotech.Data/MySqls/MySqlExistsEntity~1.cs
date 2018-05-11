using System;

namespace Lotech.Data.MySqls
{
    using static MySqlDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class MySqlExistsEntity<TEntity> : Operations.Common.CommonExistsEntity<TEntity>, IOperationProvider<Func<IDatabase, TEntity, bool>>
       where TEntity : class
    {
        public MySqlExistsEntity() : base(Quote, BuildParameter) { }
    }
}
