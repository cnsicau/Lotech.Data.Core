using Lotech.Data.Queries;
using System;
using System.Data;

namespace Lotech.Data
{
    /// <summary>
    /// SqlQuery 扩展
    /// </summary>
    public static class SqlQueryExtensions
    {
        #region Instance Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        static public ISqlQuery SqlQuery(this IDatabase database)
        {
            if (database == null) throw new NullReferenceException(nameof(database));

            return new SqlQuery(database);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        static public ISqlQuery SqlQuery(this IDatabase database, string sql)
        {
            if (database == null) throw new NullReferenceException(nameof(database));

            return database.SqlQuery().Append(sql);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        static public ISqlQuery SqlQuery(this IDatabase database, string sql, params object[] args)
        {
            if (database == null) throw new NullReferenceException(nameof(database));

            return database.SqlQuery().Append(sql, args);
        }

        #endregion

        #region Append**** Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLine(this ISqlQuery query)
        {
            return query.Append(Environment.NewLine);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLine(this ISqlQuery query, string snippet)
        {
            return query.Append(snippet).AppendLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="snippet"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLine(this ISqlQuery query, string snippet, params object[] args)
        {
            return query.Append(snippet, args).AppendLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendIf(this ISqlQuery query, bool predicate, string snippet)
        {
            if (predicate)
            {
                return query.Append(snippet);
            }
            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <param name="snippet"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ISqlQuery AppendIf(this ISqlQuery query, bool predicate, string snippet, params object[] args)
        {
            if (predicate)
            {
                return query.Append(snippet, args);
            }
            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLineIf(this ISqlQuery query, bool predicate, string snippet)
        {
            if (predicate)
            {
                return query.AppendLine(snippet);
            }
            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <param name="snippet"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLineIf(this ISqlQuery query, bool predicate, string snippet, params object[] args)
        {
            if (predicate)
            {
                return query.AppendLine(snippet, args);
            }
            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameter"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendNotNull(this ISqlQuery query, object parameter, string snippet)
        {
            if (parameter != null)
            {
                return query.Append(snippet, new[] { parameter });
            }
            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameter"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLineNotNull(this ISqlQuery query, object parameter, string snippet)
        {
            if (parameter != null)
            {
                return query.AppendLine(snippet, new[] { parameter });
            }
            return query;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameter"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendNotNullOrEmpty(this ISqlQuery query, string parameter, string snippet)
        {
            if (parameter != null && parameter.Length > 0)
            {
                return query.Append(snippet, new[] { parameter });
            }
            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameter"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLineNotNullOrEmpty(this ISqlQuery query, string parameter, string snippet)
        {
            if (parameter != null && parameter.Length > 0)
            {
                return query.AppendLine(snippet, new[] { parameter });
            }
            return query;
        }
        #endregion

        #region Execute**** Methods

        /// <summary>
        /// 执行指定SQL，并传入给定参数
        /// </summary>
        /// <example>db.ExecuteSqlReader("SELECT * FROM example WHERE Name LIKE {0} +'%'", "ad")</example>
        /// <param name="database"></param>
        /// <param name="sql">SQL语句，可使用 {0} 作为参数占位引用后续参数值</param>
        /// <param name="args">参数清单</param>
        /// <returns></returns>
        static public IDataReader ExecuteSqlReader(this IDatabase database, string sql, params object[] args)
        {
            return database.SqlQuery(sql, args).ExecuteReader();
        }

        /// <summary>
        /// 执行指定SQL，并传入给定参数
        /// </summary>
        /// <example>db.ExecuteSqlScalar("SELECT * FROM example WHERE Name LIKE {0} +'%'", "ad")</example>
        /// <param name="database"></param>
        /// <param name="sql">SQL语句，可使用 {0} 作为参数占位引用后续参数值</param>
        /// <param name="args">参数清单</param>
        /// <returns></returns>
        static public object ExecuteSqlScalar(this IDatabase database, string sql, params object[] args)
        {
            return database.SqlQuery(sql, args).ExecuteScalar();
        }

        /// <summary>
        /// 执行指定SQL，并传入给定参数
        /// </summary>
        /// <example>db.ExecuteSqlDataSet("SELECT * FROM example WHERE Name LIKE {0} +'%'", "ad")</example>
        /// <param name="database"></param>
        /// <param name="sql">SQL语句，可使用 {0} 作为参数占位引用后续参数值</param>
        /// <param name="args">参数清单</param>
        /// <returns></returns>
        static public DataSet ExecuteSqlDataSet(this IDatabase database, string sql, params object[] args)
        {
            return database.SqlQuery(sql, args).ExecuteDataSet();
        }

        /// <summary>
        /// 执行指定SQL，并传入给定参数
        /// </summary>
        /// <example>db.ExecuteSqlNonQuery("SELECT * FROM example WHERE Name LIKE {0} +'%'", "ad")</example>
        /// <param name="database"></param>
        /// <param name="sql">SQL语句，可使用 {0} 作为参数占位引用后续参数值</param>
        /// <param name="args">参数清单</param>
        static public void ExecuteSqlNonQuery(this IDatabase database, string sql, params object[] args)
        {
            database.SqlQuery(sql, args).ExecuteNonQuery();
        }

        /// <summary>
        /// 执行指定SQL，并传入给定参数
        /// </summary>
        /// <example>db.ExecuteSqlEntity("SELECT * FROM example WHERE Name LIKE {0} +'%'", "ad")</example>
        /// <param name="database"></param>
        /// <param name="sql">SQL语句，可使用 {0} 作为参数占位引用后续参数值</param>
        /// <param name="args">命名参数如:   new { name = "OK", code = "ok" }</param>
        /// <returns></returns>
        static public dynamic ExecuteSqlEntity(this IDatabase database, string sql, params object[] args)
        {
            return database.SqlQuery(sql, args).ExecuteEntity();
        }

        /// <summary>
        /// 执行指定SQL，并传入给定参数
        /// </summary>
        /// <example>db.ExecuteSqlEntities("SELECT * FROM example WHERE Name LIKE {0} +'%'", "ad")</example>
        /// <param name="database"></param>
        /// <param name="sql">SQL语句，可使用 {0} 作为参数占位引用后续参数值</param>
        /// <param name="args">命名参数如:   new { name = "OK", code = "ok" }</param>
        /// <returns></returns>
        static public dynamic[] ExecuteSqlEntities(this IDatabase database, string sql, params object[] args)
        {
            return database.SqlQuery(sql, args).ExecuteEntities();
        }

        /// <summary>
        /// 执行指定SQL，并传入给定参数
        /// </summary>
        /// <example>db.ExecuteSqlEntity("SELECT * FROM example WHERE Name LIKE {0} +'%'", "ad")</example>
        /// <param name="database"></param>
        /// <param name="sql">SQL语句，可使用 {0} 作为参数占位引用后续参数值</param>
        /// <param name="args">命名参数如:   new { name = "OK", code = "ok" }</param>
        /// <returns></returns>
        static public TEntity ExecuteSqlEntity<TEntity>(this IDatabase database, string sql, params object[] args) where TEntity : class
        {
            return database.SqlQuery(sql, args).ExecuteEntity<TEntity>();
        }

        /// <summary>
        /// 执行指定SQL，并传入给定参数
        /// </summary>
        /// <example>db.ExecuteSqlEntities("SELECT * FROM example WHERE Name LIKE {0} +'%'", "ad")</example>
        /// <param name="database"></param>
        /// <param name="sql">SQL语句，可使用 {0} 作为参数占位引用后续参数值</param>
        /// <param name="args">命名参数如:   new { name = "OK", code = "ok" }</param>
        /// <returns></returns>
        static public TEntity[] ExecuteSqlEntities<TEntity>(this IDatabase database, string sql, params object[] args) where TEntity : class
        {
            return database.SqlQuery(sql, args).ExecuteEntities<TEntity>();
        }
        #endregion
    }
}
