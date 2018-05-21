using System.Linq;

namespace Lotech.Data.Oracles
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
        public static PageData<T> PageExecuteEntites<T>(this ISqlQuery query, Page page) where T : class
        {
            var count = query.Database.SqlQuery("SELECT COUNT(1) FROM (").Append(query).Append(")").ExecuteScalar<int>();
            // 无数据
            if (count == 0) return new PageData<T>(0, new T[0]);

            string orderBy = "1";
            if (page.Orders?.Length > 0)
            {
                orderBy = string.Join(", ", page.Orders.Select(_ => query.Database.QuoteName(_.Column) + " " + _.Direction));
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
                result.Data = query.Database.SqlQuery("SELECT * FROM (SELECT i.*, i.ROWNUM AS \"__RowIndex\" FROM (")
                                    .Append(query)
                                    .Append(" ORDER BY ").Append(orderBy)
                                    .Append(") i WHERE ROWNUM <= ").Append(((page.Index + 1) * page.Size).ToString())
                                    .Append(") o WHERE \"__RowIndex\" > ").Append((page.Index * page.Size).ToString())
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
            where T : class
        {
            return db.SqlQuery(sql, args).PageExecuteEntites<T>(page);
        }
    }
}
