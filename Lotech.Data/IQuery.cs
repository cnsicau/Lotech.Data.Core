using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Lotech.Data
{
    /// <summary>
    /// 查询
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// 创建用于执行查询的 DbCommand
        /// </summary>
        /// <returns></returns>
        DbCommand CreateCommand();

        /// <summary>
        /// 获取该查询所属库
        /// </summary>
        IDatabase Database { get; }

        #region Execute****

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        DataSet ExecuteDataSet();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        dynamic[] ExecuteEntities();


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        TEntity[] ExecuteEntities<TEntity>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        dynamic ExecuteEntity();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        TEntity ExecuteEntity<TEntity>();

        /// <summary>
        /// 
        /// </summary>
        int ExecuteNonQuery();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IDataReader ExecuteReader();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        TScalar ExecuteScalar<TScalar>();


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        object ExecuteScalar();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IEnumerable<TEntity> ExecuteEnumerable<TEntity>();

        #endregion
    }
}
