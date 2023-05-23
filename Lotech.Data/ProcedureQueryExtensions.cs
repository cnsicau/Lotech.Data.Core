using System.Data;

namespace Lotech.Data
{
    /// <summary>
    /// 
    /// </summary>
    public static class ProcedureQueryExtensions
    {
        #region Execute**** Methods

        /// <summary>
        /// 执行指定存储过程，并使用命名参数绑定
        /// </summary>
        /// <example>db.ExecuteProcedureReader("proc", org => new { name = 4 })</example>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="database"></param>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="namedParameter">命名参数如:  new { name = "OK", code = "ok" }</param>
        /// <returns></returns>
        /// <returns></returns>
        static public IDataReader ExecuteProcedureReader<TParameter>(this IDatabase database, string procedureName, TParameter namedParameter) where TParameter : class
        {
            return database.ProcedureQuery(procedureName, namedParameter).ExecuteReader();
        }

        /// <summary>
        /// 执行指定存储过程，并使用命名参数绑定
        /// </summary>
        /// <example>db.ExecuteProcedureScalar("proc", org => new { name = 4 })</example>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="database"></param>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="namedParameter">命名参数如:  new { name = "OK", code = "ok" }</param>
        static public object ExecuteProcedureScalar<TParameter>(this IDatabase database, string procedureName, TParameter namedParameter) where TParameter : class
        {
            return database.ProcedureQuery(procedureName, namedParameter).ExecuteScalar();
        }

        /// <summary>
        /// 执行指定存储过程，并使用命名参数绑定
        /// </summary>
        /// <example>db.ExecuteDataSet("proc", org => new { name = 4 })</example>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="database"></param>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="namedParameter">命名参数如:  new { name = "OK", code = "ok" }</param>
        /// <returns></returns>
        static public DataSet ExecuteProcedureDataSet<TParameter>(this IDatabase database, string procedureName, TParameter namedParameter) where TParameter : class
        {
            return database.ProcedureQuery(procedureName, namedParameter).ExecuteDataSet();
        }

        /// <summary>
        /// 执行指定存储过程，并使用命名参数绑定
        /// </summary>
        /// <example>db.ExecuteNonQuery("proc", org => new { name = 4 })</example>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="database"></param>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="namedParameter">命名参数如:  new { name = "OK", code = "ok" }</param>
        static public void ExecuteProcedureNonQuery<TParameter>(this IDatabase database, string procedureName, TParameter namedParameter) where TParameter : class
        {
            database.ProcedureQuery(procedureName, namedParameter).ExecuteNonQuery();
        }

        /// <summary>
        /// 执行指定存储过程，并使用命名参数绑定
        /// </summary>
        /// <example>db.ExecuteEntity("proc", org => new { name = 4 })</example>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="database"></param>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="namedParameter">命名参数如:   new { name = "OK", code = "ok" }</param>
        /// <returns></returns>
        static public dynamic ExecuteProcedureEntity<TParameter>(this IDatabase database, string procedureName, TParameter namedParameter) where TParameter : class
        {
            return database.ProcedureQuery(procedureName, namedParameter).ExecuteEntity();
        }

        /// <summary>
        /// 执行指定存储过程，并使用命名参数绑定
        /// </summary>
        /// <example>db.ExecuteProcedureEntities("proc", org => new { name = 4 })</example>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="database"></param>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="namedParameter">命名参数如:   new { name = "OK", code = "ok" }</param>
        /// <returns></returns>
        static public dynamic[] ExecuteProcedureEntities<TParameter>(this IDatabase database, string procedureName, TParameter namedParameter) where TParameter : class
        {
            return database.ProcedureQuery(procedureName, namedParameter).ExecuteEntities();
        }
        #endregion
    }
}
