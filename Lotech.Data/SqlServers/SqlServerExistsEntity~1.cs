using System;

namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class SqlServerExistsEntity<TEntity> : Operations.Common.CommonExistsEntity<TEntity>
       where TEntity : class
    {
        public SqlServerExistsEntity() : base(Quote, BuildParameter) { }
    }
}
