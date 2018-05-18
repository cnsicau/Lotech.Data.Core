using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;

namespace Lotech.Data
{
    /// <summary>
    /// 数据库操作接口
    /// </summary>
    public interface IDatabase
    {
        #region Properties
        /// <summary>
        /// 日志记录
        /// </summary>
        Action<string> Log { get; set; }
        /// <summary>
        /// 获取连接字符串
        /// </summary>
        string ConnectionString
        {
            get;
            set;
        }
        #endregion

        #region Connection
        /// <summary>
        /// 创建连接对象（非打开）
        /// </summary>
        /// <returns></returns>
        DbConnection CreateConnection();
        #endregion

        #region Command
        /// <summary>
        /// 获取指定类型的命令对象
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        DbCommand GetCommand(CommandType commandType, string commandText);
        /// <summary>
        /// 获取SQL命令对象
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        DbCommand GetSqlStringCommand(string query);
        /// <summary>
        /// 获取存储过程命令对象
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        DbCommand GetStoredProcedureCommand(string procedureName);
        #endregion

        #region Parameters
        /// <summary>
        /// 添加指定方向的参数
        /// </summary>
        /// <param name="command">命令对象</param>
        /// <param name="parameterName">参数名称</param>
        /// <param name="dbType">参数的数据库类型</param>
        /// <param name="direction">参数方向（输入或输出）</param>
        /// <param name="value">参数值</param>
        void AddParameter(DbCommand command, string parameterName, DbType dbType, ParameterDirection direction, object value);
        /// <summary>
        /// 添加复杂参数
        /// </summary>
        /// <param name="command">命令对象</param>
        /// <param name="parameterName">参数名称</param>
        /// <param name="dbType">参数的数据库类型</param>
        /// <param name="direction">参数方向（输入或输出）</param>
        /// <param name="size">参数值长度</param>
        /// <param name="nullable">是否可空</param>
        /// <param name="precision">长度</param>
        /// <param name="scale">精度</param>
        /// <param name="value">参数值</param>
        void AddParameter(DbCommand command, string parameterName, DbType dbType, ParameterDirection direction, int size, bool nullable, int precision, int scale, object value);
        /// <summary>
        /// 添加输入参数
        /// </summary>
        /// <param name="command">命令对象</param>
        /// <param name="parameterName">参数名称</param>
        /// <param name="dbType">参数的数据库类型</param>
        /// <param name="value">参数值</param>
        void AddInParameter(DbCommand command, string parameterName, DbType dbType, object value);
        /// <summary>
        /// 添加输出参数
        /// </summary>
        /// <param name="command">命令对象</param>
        /// <param name="parameterName">参数名称</param>
        /// <param name="dbType">参数的数据库类型</param>
        /// <param name="size">参数长度</param>
        void AddOutParameter(DbCommand command, string parameterName, DbType dbType, int size);
        /// <summary>
        /// 创建参数名称，如SQLServer返回"@name", Oracle返回":name"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string BuildParameterName(string name);

        /// <summary>
        /// 引述名称  SqlServer中使用 [], MySQL 使用 ``，Oracle使用 ""
        ///     用于解决关键字的处理
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string QuoteName(string name);
        #endregion

        #region ExecuteReader
        /// <summary>
        /// 执行Reader输出
        /// </summary>
        /// <param name="command">命令对象</param>
        /// <returns>返回IDataReader对象</returns>
        IDataReader ExecuteReader(DbCommand command);

        /// <summary>
        /// 执行Reader输出
        /// </summary>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回IDataReader对象</returns>
        IDataReader ExecuteReader(CommandType commandType, string commandText);

        /// <summary>
        /// 执行命令语句
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns>返回IDataReader对象</returns>
        IDataReader ExecuteReader(string commandText);
        #endregion

        #region ExecuteScalar
        /// <summary>
        /// 执行单一输出
        /// </summary>
        /// <param name="command">命令对象</param>
        /// <returns>返回返回单一对象值, 没有值时返回null</returns>
        object ExecuteScalar(DbCommand command);

        /// <summary>
        /// 执行单一输出
        /// </summary>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回单一对象值, 没有值时返回null</returns>
        object ExecuteScalar(CommandType commandType, string commandText);


        /// <summary>
        /// 执行单一输出
        /// </summary>
        /// <param name="commandText">命令类型</param>
        /// <returns>返回单一对象值, 没有值时返回null</returns>
        object ExecuteScalar(string commandText);

        /// <summary>
        /// 执行单一输出指定类型
        /// </summary>
        /// <typeparam name="TScalar">返回类型</typeparam>
        /// <param name="command">命令对象</param>
        /// <returns>返回返回单一对象值, 没有值时返回null</returns>
        TScalar ExecuteScalar<TScalar>(DbCommand command);

        /// <summary>
        /// 执行单一输出指定类型
        /// </summary>
        /// <typeparam name="TScalar">返回类型</typeparam>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回单一对象值, 没有值时返回null</returns>
        TScalar ExecuteScalar<TScalar>(string commandText);

        /// <summary>
        /// 执行单一输出指定类型
        /// </summary>
        /// <typeparam name="TScalar">返回类型</typeparam>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回单一对象值, 没有值时返回null</returns>
        TScalar ExecuteScalar<TScalar>(CommandType commandType, string commandText);
        #endregion

        #region ExecuteDataSet
        /// <summary>
        /// 执行DataSet输出
        /// </summary>
        /// <param name="command">命令对象</param>
        /// <returns>返回DataSet对象</returns>
        DataSet ExecuteDataSet(DbCommand command);

        /// <summary>
        /// 执行DataSet输出
        /// </summary>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回DataSet对象</returns>
        DataSet ExecuteDataSet(string commandText);

        /// <summary>
        /// 执行DataSet输出
        /// </summary>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回DataSet对象</returns>
        DataSet ExecuteDataSet(CommandType commandType, string commandText);
        #endregion

        #region ExecuteNonQuery
        /// <summary>
        /// 执行非查询（而是如更新，删除等）
        /// </summary>
        /// <param name="command">命令对象</param>
        /// <returns>返回受影响的行数（如果数据库关闭了影响行，结果将无效）</returns>
        int ExecuteNonQuery(DbCommand command);

        /// <summary>
        /// 执行非查询（而是如更新，删除等）
        /// </summary>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回受影响的行数（如果数据库关闭了影响行，结果将无效）</returns>
        int ExecuteNonQuery(string commandText);

        /// <summary>
        /// 执行非查询（而是如更新，删除等）
        /// </summary>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回受影响的行数（如果数据库关闭了影响行，结果将无效）</returns>
        int ExecuteNonQuery(CommandType commandType, string commandText);
        #endregion

        #region EntitiesMethod

        #region Exists
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        bool Exists<EntityType, TKey>(TKey value) where EntityType : class;
        /// <summary>
        /// 判断数据是否已经存在
        /// </summary>
        /// <typeparam name="EntityType">实体类型</typeparam>
        /// <param name="entity">包含主键值的实体</param>
        /// <returns>返回主键对应的实体，不存在时返回null</returns>
        bool Exists<EntityType>(EntityType entity) where EntityType : class;
        /// <summary>
        /// 按条件判断数据是否已经存在（可用于对复合主键数据进行加载）
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        bool Exists<EntityType>(Expression<Func<EntityType, bool>> predicate) where EntityType : class;
        #endregion

        #region Entity
        /// <summary>
        /// 执行返回指定实体
        /// </summary>
        /// <typeparam name="EntityType">实体类型</typeparam>
        /// <param name="command">命令</param>
        /// <returns></returns>
        EntityType ExecuteEntity<EntityType>(DbCommand command) where EntityType : class;

        /// <summary>
        /// 执行返回指定实体集合
        /// </summary>
        /// <typeparam name="EntityType">实体类型</typeparam>
        /// <param name="command">命令</param>
        /// <returns>返回可枚举的实体集合</returns>
        EntityType[] ExecuteEntities<EntityType>(DbCommand command) where EntityType : class;
        /// <summary>
        /// 执行返回指定实体
        /// </summary>
        /// <typeparam name="EntityType">实体类型</typeparam>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns></returns>
        EntityType ExecuteEntity<EntityType>(CommandType commandType, string commandText) where EntityType : class;

        /// <summary>
        /// 执行返回指定实体集合
        /// </summary>
        /// <typeparam name="EntityType">实体类型</typeparam>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回可枚举的实体集合</returns>
        EntityType[] ExecuteEntities<EntityType>(CommandType commandType, string commandText) where EntityType : class;
        /// <summary>
        /// 执行返回指定实体
        /// </summary>
        /// <typeparam name="EntityType">实体类型</typeparam>
        /// <param name="commandText">命令文本</param>
        /// <returns></returns>
        EntityType ExecuteEntity<EntityType>(string commandText) where EntityType : class;

        /// <summary>
        /// 执行返回指定实体集合
        /// </summary>
        /// <typeparam name="EntityType">实体类型</typeparam>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回可枚举的实体集合</returns>
        EntityType[] ExecuteEntities<EntityType>(string commandText) where EntityType : class;
        #endregion

        #region Object Entity
        /// <summary>
        /// 执行返回指定实体
        /// </summary>
        /// <param name="command">命令</param>
        /// <returns></returns>
        dynamic ExecuteEntity(DbCommand command);

        /// <summary>
        /// 执行返回指定实体集合
        /// </summary>
        /// <param name="command">命令</param>
        /// <returns>返回可枚举的实体集合</returns>
        dynamic[] ExecuteEntities(DbCommand command);
        /// <summary>
        /// 执行返回指定实体
        /// </summary>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns></returns>
        dynamic ExecuteEntity(CommandType commandType, string commandText);

        /// <summary>
        /// 执行返回指定实体集合
        /// </summary>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回可枚举的实体集合</returns>
        dynamic[] ExecuteEntities(CommandType commandType, string commandText);
        /// <summary>
        /// 执行返回指定实体
        /// </summary>
        /// <param name="commandText">命令文本</param>
        /// <returns></returns>
        dynamic ExecuteEntity(string commandText);

        /// <summary>
        /// 执行返回指定实体集合
        /// </summary>
        /// <param name="commandText">命令文本</param>
        /// <returns>返回可枚举的实体集合</returns>
        dynamic[] ExecuteEntities(string commandText);
        #endregion

        #region Insert
        /// <summary>
        /// 创建实体（INSERT）
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entity"></param>
        void InsertEntity<EntityType>(EntityType entity) where EntityType : class;
        /// <summary>
        /// 批量创建实体（INSERT）
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entities"></param>
        void InsertEntities<EntityType>(IEnumerable<EntityType> entities) where EntityType : class;

        #endregion

        #region Update

        /// <summary>
        /// 更新实体(UPDATE)
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entity"></param>
        void UpdateEntity<EntityType>(EntityType entity) where EntityType : class;

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entities"></param>
        void UpdateEntities<EntityType>(IEnumerable<EntityType> entities) where EntityType : class;

        /// <summary>
        /// 仅更新包含部分
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TInclude"></typeparam>
        /// <param name="entity"></param>
        /// <param name="include">仅更新部分，如  _ => new { _.ModifyTime, _.Deleted } </param>
        void UpdateEntityInclude<EntityType, TInclude>(EntityType entity, Func<EntityType, TInclude> include) where EntityType : class where TInclude : class;

        /// <summary>
        /// 仅更新包含部分
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TInclude"></typeparam>
        /// <param name="entities"></param>
        /// <param name="include">仅更新部分，如  _ => new { _.ModifyTime, _.Deleted } </param>
        void UpdateEntitiesInclude<EntityType, TInclude>(IEnumerable<EntityType> entities, Func<EntityType, TInclude> include) where EntityType : class where TInclude : class;

        /// <summary>
        /// 更新除指定内容部分
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TExclude"></typeparam>
        /// <param name="entity"></param>
        /// <param name="exclude">排除部分，如  _ => new { _.Code, _.CreateTime } </param>
        void UpdateEntityExclude<EntityType, TExclude>(EntityType entity, Func<EntityType, TExclude> exclude) where EntityType : class where TExclude : class;

        /// <summary>
        /// 更新除指定内容部分
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TExclude"></typeparam>
        /// <param name="entities"></param>
        /// <param name="exclude">排除部分，如  _ => new { _.Code, _.CreateTime } </param>
        void UpdateEntitiesExclude<EntityType, TExclude>(IEnumerable<EntityType> entities, Func<EntityType, TExclude> exclude) where EntityType : class where TExclude : class;

        /// <summary>
        /// 按条件批量更新数据, 如 db.UpdateEntities(deleted, _=> new {_.Deleted }, _ => _.Code.StartsWith('9'));
        /// </summary>
        /// <example>
        ///     <![CDATA[
        ///         // 将所有 Code 以 9开头的 数据行的 Is_Delted 字段更新为 删除
        ///         db.UpdateEntities(new Dic_Organiztion{Id = 1, Is_Delted = true }, entity => new { entity.Is_Delted }, _ => _.Code.StartsWith('9');
        ///     ]]>
        /// </example>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TSet"></typeparam>
        /// <param name="entity"></param>
        /// <param name="sets"></param>
        /// <param name="predicate">条件</param>
        void UpdateEntities<EntityType, TSet>(EntityType entity, Func<EntityType, TSet> sets, Expression<Func<EntityType, bool>> predicate) where EntityType : class where TSet : class;
        #endregion

        #region Delete

        /// <summary>
        /// 按主键删除数据
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="id"></param>
        void DeleteEntity<EntityType, TKey>(TKey id) where EntityType : class;

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entity"></param>
        void DeleteEntity<EntityType>(EntityType entity) where EntityType : class;

        /// <summary>
        /// 删除实体集合
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entities"></param>
        void DeleteEntities<EntityType>(IEnumerable<EntityType> entities) where EntityType : class;
        /// <summary>
        /// 删除符合条件的集合
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="conditions"></param>
        void DeleteEntities<EntityType>(Expression<Func<EntityType, bool>> conditions) where EntityType : class;
        #endregion

        #region Find

        /// <summary>
        /// 查找给定条件的数据
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="conditions"></param>
        /// <returns></returns>
        EntityType[] FindEntities<EntityType>(Expression<Func<EntityType, bool>> conditions) where EntityType : class;

        /// <summary>
        /// 查找所有数据
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <returns></returns>
        EntityType[] FindEntities<EntityType>() where EntityType : class;
        #endregion

        #region Count
        /// <summary>
        /// 获取满足条件的记录数
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="conditions"></param>
        /// <returns></returns>
        int Count<EntityType>(Expression<Func<EntityType, bool>> conditions) where EntityType : class;

        /// <summary>
        /// 获取记录总数
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <returns></returns>
        int Count<EntityType>() where EntityType : class;
        #endregion

        #region Load
        /// <summary>
        /// 加载单一主键实体
        /// </summary>
        /// <typeparam name="EntityType">实体类型</typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="key">主键值</param>
        /// <returns>返回主键对应的实体，不存在时返回null</returns>
        EntityType LoadEntity<EntityType, TKey>(TKey key) where EntityType : class;

        /// <summary>
        /// 加载实体键对应的数据
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entity">包含主键的实体</param>
        /// <returns></returns>
        EntityType LoadEntity<EntityType>(EntityType entity) where EntityType : class;

        /// <summary>
        /// 按条件加载数据实体（可用于对复合主键数据进行加载）
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        EntityType LoadEntity<EntityType>(Expression<Func<EntityType, bool>> predicate) where EntityType : class;
        #endregion
        #endregion
    }
}
