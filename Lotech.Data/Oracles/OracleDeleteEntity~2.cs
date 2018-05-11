namespace Lotech.Data.Oracles
{
    using static OracleDatabase;
    /// <summary>
    /// 按主键删除
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class OracleDeleteEntity<TEntity, TKey> : Operations.Common.CommonDeleteEntity<TEntity, TKey>
       where TEntity : class
    {
        public OracleDeleteEntity() : base(Quote, BuildParameter) { }
    }
}
