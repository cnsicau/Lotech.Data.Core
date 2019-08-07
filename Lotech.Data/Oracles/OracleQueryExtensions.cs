using System;
using System.Linq;
using System.Linq.Expressions;
#if ORACLE
using Lotech.Data.Oracles;

namespace Lotech.Data
#else
namespace Lotech.Data.Oracles
#endif
{
    /// <summary>
    /// 
    /// </summary>
    public static class OracleQueryExtensions
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
            var count = query.Database.SqlQuery("/*CountQuery*/SELECT COUNT(1) FROM (")
                            .AppendLine().AppendLine(query).Append("/*~CountQuery*/)")
                            .ExecuteScalar<int>();
            // 无数据
            if (count == 0) return new PageData<T>(0, new T[0]);

            string orderBy = "1";
            if (page.Orders?.Length > 0)
            {
                // 取消排序列的 Quote 引述，避免"Id" 无法自动转大写，引起额外的工作量
                orderBy = string.Join(", ", page.Orders.Select(_ => _.Column + " " + _.Direction));
            }

            var result = new PageData<T>(count);
            if (page.Index == 0)
            {
                result.Data = query.Database.SqlQuery("SELECT * FROM (")
                                    .Append(query)
                                    .Append(" ORDER BY ").Append(orderBy)
                                    .Append(") t WHERE ROWNUM <= ").Append(page.Size.ToString())
                                    .ExecuteEntities<T>();
            }
            else
            {
                result.Data = query.Database.SqlQuery("/*DataQuery*/SELECT * FROM (SELECT i.*, ROWNUM AS \"__RowIndex\" FROM (").AppendLine()
                                    .AppendLine(query)
                                    .Append(" ORDER BY ").AppendLine(orderBy)
                                    .Append("/*~DataQuery*/) i WHERE ROWNUM <= ").Append((page.Index + 1) * page.Size)
                                    .Append(") o WHERE \"__RowIndex\" > ").Append(page.Index * page.Size)
                                    .ExecuteEntities<T>();
            }

            return result;
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
            return query.AppendExpression(new OracleExpressionVisitor<T>(query.Database, Descriptors.Operation.Select), predicate);
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
