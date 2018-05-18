using Lotech.Data.Queries;
using System;
using System.Data;
using System.Data.Common;

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

        #region Database Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static DbCommand CreateCommand(this ISqlQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");
            var command = query.Database.GetSqlStringCommand(query.GetSnippets());

            foreach (var p in query.GetParameters())
            {
                var type = p.Value?.GetType() ?? typeof(string);
                query.Database.AddInParameter(command, p.Key, Utils.DbTypeParser.Parse(type), p.Value);
            }
            return command;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static DataSet ExecuteDataSet(this ISqlQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteDataSet(query.CreateCommand());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static dynamic[] ExecuteEntities(this ISqlQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteEntities(query.CreateCommand());
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static TEntity[] ExecuteEntities<TEntity>(this ISqlQuery query) where TEntity : class
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteEntities<TEntity>(query.CreateCommand());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static dynamic ExecuteEntity(this ISqlQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteEntity(query.CreateCommand());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static TEntity ExecuteEntity<TEntity>(this ISqlQuery query) where TEntity : class
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteEntity<TEntity>(query.CreateCommand());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        public static void ExecuteNonQuery(this ISqlQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            query.Database.ExecuteNonQuery(query.CreateCommand());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IDataReader ExecuteReader(this ISqlQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteReader(query.CreateCommand());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static TScalar ExecuteScalar<TScalar>(this ISqlQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteScalar<TScalar>(query.CreateCommand());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static object ExecuteScalar(this ISqlQuery query)
        {
            if (query.Database == null) throw new InvalidOperationException("必须给出 query 的 Database");

            return query.Database.ExecuteScalar(query.CreateCommand());
        }
        #endregion
    }
}
