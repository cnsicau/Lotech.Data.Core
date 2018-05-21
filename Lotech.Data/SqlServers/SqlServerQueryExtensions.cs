﻿using System.Linq;

namespace Lotech.Data.SqlServers
{
    /// <summary>
    /// 
    /// </summary>
    public static class SqlServerQueryExtensions
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
            var count = query.Database.SqlQuery("SELECT COUNT(1) FROM（").Append(query).Append(") t").ExecuteScalar<int>();
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
                result.Data = query.Database.SqlQuery("SELECT TOP(").Append(page.Size.ToString())
                                        .Append(") * FROM (")
                                        .Append(query).Append(") t ORDER BY ").Append(orderBy)
                                        .ExecuteEntities<T>();
            }
            else
            {
                result.Data = query.Database.SqlQuery("SELECT *, ROW_NUMBER(ORDER BY ")
                                    .Append(orderBy).Append(") as __RowIndex FROM (")
                                    .Append(query).Append(") t WHERE __RowIndex BETWEEN ")
                                    .Append((page.Size * page.Index).ToString())
                                    .Append(" AND ").Append((page.Size * (page.Index + 1)).ToString())
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
