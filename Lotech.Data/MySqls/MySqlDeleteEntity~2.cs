﻿namespace Lotech.Data.MySqls
{
    using static MySqlEntityServices;
    /// <summary>
    /// 按主键删除
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    class MySqlDeleteEntity<TEntity, TKey> : Operations.Common.CommonDeleteEntity<TEntity, TKey>
       where TEntity : class
    {
        public MySqlDeleteEntity() : base(Quote, BuildParameter) { }
    }
}
