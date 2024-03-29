﻿namespace Lotech.Data.SQLites
{
    using static SQLiteEntityServices;
    /// <summary>
    /// 按主键删除
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class SQLiteDeleteEntity<TEntity, TKey> : Operations.Common.CommonDeleteEntity<TEntity, TKey>
       where TEntity : class
    {
        public SQLiteDeleteEntity() : base(Quote, BuildParameter) { }
    }
}
