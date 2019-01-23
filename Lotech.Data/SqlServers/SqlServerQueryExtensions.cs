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
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static PageData<T> PageExecuteEntites<T>(this ISqlQuery query, Page page) where T : class
        {
            if (page.Index > 0 && (page.Orders == null || page.Orders.Length == 0))
                throw new InvalidOperationException("由于第2页之后使用 ROW_NUMBER() OVER(ORDER BY ***)分页，必须给出至少一个排序字段.");

            var count = query.Database.SqlQuery("/*CountQuery*/SELECT COUNT(1) FROM (")
                            .AppendLine().AppendLine(query).Append("/*~CountQuery*/) t")
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
                result.Data = query.Database.SqlQuery("/*DataQuery*/SELECT TOP(").Append(page.Size.ToString())
                                        .AppendLine(") * FROM (")
                                        .AppendLine(query)
                                        .Append("/*~DataQuery*/) t ORDER BY ").Append(orderBy)
                                        .ExecuteEntities<T>();
            }
            else
            {
                result.Data = query.Database.SqlQuery(@"/*DataQuery*/SELECT * FROM (")
                                .Append("SELECT *, ROW_NUMBER() OVER(ORDER BY ").Append(orderBy).Append(@")  AS __RowIndex ")
                                .AppendLine("FROM (")
                                .AppendLine(query)
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
            where T : class
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
