namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;
    /// <summary>
    /// 按主键删除
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class SqlServerDeleteEntity<TEntity, TKey> : Operations.Common.CommonDeleteEntity<TEntity, TKey>
       where TEntity : class
    {
        public SqlServerDeleteEntity() : base(Quote, BuildParameter) { }
    }
}
