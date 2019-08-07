using System;
using System.Linq;
using System.Linq.Expressions;

#if MYSQL
using Lotech.Data.MySqls;

namespace Lotech.Data
#else
namespace Lotech.Data.MySqls
#endif
{
    /// <summary>
    /// 
    /// </summary>
    public static class MySqlQueryExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static PageData<T> PageExecuteEntites<T>(this ISqlQuery query, Page page)
        {
            var count = query.Database.SqlQuery("SELECT COUNT(1) FROM (").Append(query).Append(") t").ExecuteScalar<int>();
            // 无数据
            if (count == 0) return new PageData<T>(0, new T[0]);

            string orderBy = "1";
            if (page.Orders?.Length > 0)
            {
                orderBy = string.Join(", ", page.Orders.Select(_ => query.Database.QuoteName(_.Column) + " " + _.Direction));
            }

            var data = query.Database.SqlQuery("SELECT * FROM (")
                                    .Append(query)
                                    .Append(") t ORDER BY ").Append(orderBy)
                                    .Append(" LIMIT ").Append(page.Size.ToString())
                                    .AppendIf(page.Index > 0, " OFFSET " + (page.Index * page.Size))
                                    .ExecuteEntities<T>();

            return new PageData<T>(count, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="page"></param>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static PageData<T> PageExecuteEntities<T>(this IDatabase db, Page page, string sql, params object[] args)
        {
            return db.SqlQuery(sql, args).PageExecuteEntites<T>(page);
        }

        /// <summary>
        /// 通过 Lamda 表达式添加条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static ISqlQuery Append<T>(this ISqlQuery query, Expression<Func<T, bool>> predicate) where T : class
        {
            return query.AppendExpression(new MySqlExpressionVisitor<T>(query.Database, Descriptors.Operation.Select), predicate);
        }
        /// <summary>
        /// 通过 Lamda 表达式添加条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLine<T>(this ISqlQuery query, Expression<Func<T, bool>> predicate) where T : class
        {
            return query.Append(predicate).AppendLine();
        }
        /// <summary>
        /// 当condition为真时，添加表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// 
        public static ISqlQuery AppendIf<T>(this ISqlQuery query, bool condition, Expression<Func<T, bool>> predicate) where T : class
        {
            if (condition)
            {
                return query.Append(predicate);
            }
            return query;
        }
        /// <summary>
        /// 当condition为真时，添加表达式并换行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLineIf<T>(this ISqlQuery query, bool condition, Expression<Func<T, bool>> predicate) where T : class
        {
            if (condition)
            {
                return query.AppendLine(predicate);
            }
            return query;
        }
    }
}
