using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Lotech.Data.Operations;
#if SQLSERVER
using Lotech.Data.SqlServers;

namespace Lotech.Data
#else
namespace Lotech.Data.SqlServers
#endif
{
    /// <summary>
    /// 
    /// </summary>
    public static class SqlServerQueryExtensions
    {
        /// <summary>
        /// 计算CTE语句长度  WITH alais AS (SELECT ****), b AS (SELECT ***) , .., n AS () SELECT ...
        ///                 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>返回长度，0 代表无CTE</returns>
        static int DeterminingCteLength(string sql)
        {
            // 忽略前端空白，检查 WITH 标识符
            int i = 0, len = sql.Length - 5;
            while (i++ < len)
            {
                var chr = sql[i];
                if (char.IsWhiteSpace(chr)) continue;

                if (chr != 'W' && chr != 'w'
                    || (sql[++i] != 'I' && sql[i] != 'i')
                    || (sql[++i] != 'T' && sql[i] != 't')
                    || (sql[++i] != 'H' && sql[i] != 'h')
                    || !char.IsWhiteSpace(sql[++i])) return 0;

                break;
            }

            if (i == 0) return 0;

            var isInStr = false;
            var isInBrackets = false;
            var success = false;

            while (++i < sql.Length)
            {
                var chr = sql[i];

                if (success)
                {
                    if (chr == ','/*多个CTE分组*/)
                    {
                        success = false;
                        continue;
                    }
                    else if (!char.IsWhiteSpace(chr))
                    {
                        return i - 1;
                    }
                }

                if (isInStr) { isInStr = chr != '\''; continue; }
                else if (chr == '\'') { isInStr = true; continue; }

                if (isInBrackets) { success = chr == ')'; isInBrackets = !success; }
                else if (chr == '(') { isInBrackets = true; }
            }
            if (!success || i == sql.Length) return 0;
            return i;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static PageData<T> PageExecuteEntites<T>(this ISqlQuery query, Page page)
        {
            if (page.Index > 0 && (page.Orders == null || page.Orders.Length == 0))
                throw new InvalidOperationException("由于第2页之后使用 ROW_NUMBER() OVER(ORDER BY ***)分页，必须给出至少一个排序字段.");

            var sql = query.GetSnippets();
            var parameters = query.GetParameters();
            var cte = DeterminingCteLength(sql);

            var count = query.Database.SqlQuery(sql.Length + 80)
                            .Append("/*CountQuery*/")
                            .AppendString(sql, 0, cte)
                            .AppendLine("SELECT COUNT(1) FROM (")
                            .AppendLineRaw(sql, cte, sql.Length - cte, parameters)
                            .Append("/*~CountQuery*/) t")
                            .ExecuteScalar<int>();
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
                result.Data = query.Database.SqlQuery(sql.Length + orderBy.Length + 96)
                                .Append(@"/*DataQuery*/")
                                .AppendString(sql, 0, cte)
                                .Append("SELECT TOP(").Append(page.Size.ToString())
                                .AppendLine(") * FROM (")
                                .AppendLineRaw(sql, cte, sql.Length - cte, parameters)
                                .Append("/*~DataQuery*/) t ORDER BY ").Append(orderBy)
                                .ExecuteEntities<T>();
            }
            else
            {
                result.Data = query.Database.SqlQuery(sql.Length + orderBy.Length + 192)
                                .Append(@"/*DataQuery*/")
                                .AppendString(sql, 0, cte)
                                .Append("SELECT * FROM (")
                                .Append("SELECT *, ROW_NUMBER() OVER(ORDER BY ").Append(orderBy).Append(@")  AS __RowIndex ")
                                .AppendLine("FROM (")
                                .AppendLineRaw(sql, cte, sql.Length - cte, parameters)
                                .Append(@"/*~DataQuery*/ ) I")
                                .Append(") O WHERE __RowIndex BETWEEN ").Append(page.Size * page.Index + 1)
                                .Append(" AND ").Append(page.Size * (page.Index + 1))
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
            return query.AppendExpression(new SqlServerExpressionVisitor<T>(query.Database, Descriptors.Operation.Select), predicate);
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

        #region Bulk****
        /// <summary>
        /// 大批量插入数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="db"></param>
        /// <param name="entities"></param>
        static public void BulkInsertEntities<TEntity>(this IDatabase db, IEnumerable<TEntity> entities) where TEntity : class
        {
            Operation<TEntity, Action<IDatabase, IEnumerable<TEntity>>
                , BulkInsertOperationBuilder<TEntity>>.Instance(
                db.DescriptorProvider, Descriptors.Operation.Insert
            )(db, entities);
        }

        /// <summary>
        /// 大批量更新数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="db"></param>
        /// <param name="entities"></param>
        static public void BulkUpdateEntities<TEntity>(this IDatabase db, IEnumerable<TEntity> entities) where TEntity : class
        {
            Operation<TEntity, Action<IDatabase, IEnumerable<TEntity>>
                , BulkUpdateOperationBuilder<TEntity>>.Instance(
                db.DescriptorProvider, Descriptors.Operation.Insert
            )(db, entities);
        }

        /// <summary>
        /// 排除列大批量更新数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TExclude"></typeparam>
        /// <param name="db"></param>
        /// <param name="entities"></param>
        static public void BulkUpdateEntitiesExclude<TEntity, TExclude>(this IDatabase db, IEnumerable<TEntity> entities)
            where TEntity : class where TExclude : class
        {
            Operation<TEntity, Action<IDatabase, IEnumerable<TEntity>>
                , BulkUpdateOperationBuilder<TEntity>.Exclude<TExclude>>.Instance(
                db.DescriptorProvider, Descriptors.Operation.Insert
            )(db, entities);
        }

        /// <summary>
        /// 限制列大批量更新数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TInclude"></typeparam>
        /// <param name="db"></param>
        /// <param name="entities"></param>
        static public void BulkUpdateEntitiesInclude<TEntity, TInclude>(this IDatabase db, IEnumerable<TEntity> entities)
            where TEntity : class where TInclude : class
        {
            Operation<TEntity, Action<IDatabase, IEnumerable<TEntity>>
                , BulkUpdateOperationBuilder<TEntity>.Include<TInclude>>.Instance(
                db.DescriptorProvider, Descriptors.Operation.Insert
            )(db, entities);
        }

        /// <summary>
        /// 大批量删除数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="db"></param>
        /// <param name="entities"></param>
        static public void BulkDeleteEntities<TEntity>(this IDatabase db, IEnumerable<TEntity> entities) where TEntity : class
        {
            Operation<TEntity, Action<IDatabase, IEnumerable<TEntity>>
                , BulkDeleteOperationBuilder<TEntity>>.Instance(
                db.DescriptorProvider, Descriptors.Operation.Insert
            )(db, entities);
        }
        #endregion
    }
}
