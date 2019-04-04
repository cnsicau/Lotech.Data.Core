using Lotech.Data.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Lotech.Data
{
    /// <summary>
    /// IQuery 扩展方法
    /// </summary>
    static public class QueryExtensions
    {

        #region Extension Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static DataSet ExecuteDataSet(this IQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteDataSet(query.CreateCommand());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static dynamic[] ExecuteEntities(this IQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteEntities(query.CreateCommand());
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static TEntity[] ExecuteEntities<TEntity>(this IQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteEntities<TEntity>(query.CreateCommand());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static dynamic ExecuteEntity(this IQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteEntity(query.CreateCommand());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static TEntity ExecuteEntity<TEntity>(this IQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteEntity<TEntity>(query.CreateCommand());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        public static void ExecuteNonQuery(this IQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            query.Database.ExecuteNonQuery(query.CreateCommand());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IDataReader ExecuteReader(this IQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteReader(query.CreateCommand());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static TScalar ExecuteScalar<TScalar>(this IQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteScalar<TScalar>(query.CreateCommand());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static object ExecuteScalar(this IQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteScalar(query.CreateCommand());
        }
        #endregion

        #region ReadEntities
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        static public IEnumerable<TEntity> ExecuteEntityReader<TEntity>(this IQuery query)
        {
            var command = query.CreateCommand();
            try
            {
                return new QueryResult<TEntity>(query.Database, command
                            , DbProviderDatabase.ResultMapper<TEntity>.Create(query.Database));
            }
            catch
            {
                command.Dispose();
                throw;
            }
        }
        #endregion
    }
}
