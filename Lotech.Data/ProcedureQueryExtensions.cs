using Lotech.Data.Descriptors;
using Lotech.Data.Queries;
using Lotech.Data.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Lotech.Data
{
    /// <summary>
    /// 
    /// </summary>
    public static class ProcedureQueryExtensions
    {
        static class ModelParameter<TParameter> where TParameter : class
        {
            internal static readonly Action<IProcedureQuery, TParameter>[] binders
                = DefaultDescriptorProvider.Instance.GetEntityDescriptor<TParameter>(Operation.Select).Members
                .Select<IMemberDescriptor, Action<IProcedureQuery, TParameter>>(member =>
                {
                    var get = MemberAccessor<TParameter, object>.GetGetter(member.Member);
                    return (IProcedureQuery query, TParameter parameter) => query.AddParameter(member.Name, get(parameter));
                }).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        static public IProcedureQuery ProcedureQuery(this IDatabase database, string procedureName)
        {
            if (database == null) throw new NullReferenceException(nameof(database));
            return new ProcedureQuery(database, procedureName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="query"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        static public IProcedureQuery AddParameter<TParameter>(this IProcedureQuery query, TParameter parameter) where TParameter : class
        {
            foreach (var bind in ModelParameter<TParameter>.binders)
            {
                bind(query, parameter);
            }
            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="database"></param>
        /// <param name="procedureName"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        static public IProcedureQuery ProcedureQuery<TParameter>(this IDatabase database, string procedureName, TParameter parameter) where TParameter : class
        {
            return database.ProcedureQuery(procedureName).AddParameter(parameter);
        }

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

        /// <summary>
        /// 执行指定存储过程，并使用命名参数绑定
        /// </summary>
        /// <example>db.ExecuteProcedureEntities("proc", org => new { name = 4 })</example>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="database"></param>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="namedParameter">命名参数如:   new { name = "OK", code = "ok" }</param>
        /// <returns></returns>
        static public IEnumerable<dynamic> ReadProcedureEntities<TParameter>(this IDatabase database, string procedureName, TParameter namedParameter) where TParameter : class
        {
            return database.ProcedureQuery(procedureName, namedParameter).ReadEntities<dynamic>();
        }
        #endregion
    }
}
