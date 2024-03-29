﻿using Lotech.Data.Operations;
using Lotech.Data.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Lotech.Data
{
    /// <summary>
    /// SqlQuery 扩展
    /// </summary>
    public static class SqlQueryExtensions
    {
        #region Append**** Methods
        /// <summary>
        /// 追加片断
        /// </summary>
        /// <param name="query"></param>
        /// <param name="snippet">片断中可以使用 {0}, {1} 作为参数占位</param>
        /// <param name="args">片断中占位将被替换的参数值</param>
        /// <returns></returns>
        public static ISqlQuery AppendLine(this ISqlQuery query, string snippet, IList<object> args)
        {
            return query.Append(snippet, args).AppendLine();
        }

        /// <summary>
        /// 追加片断
        /// </summary>
        /// <param name="query"></param>
        /// <param name="snippet">片断中可以使用 {0}, {1} 作为参数占位</param>
        /// <param name="args">片断中占位将被替换的参数值</param>
        /// <returns></returns>
        public static ISqlQuery Append(this ISqlQuery query, string snippet, params object[] args)
        {
            IList<object> parameters = args;
            return query.Append(snippet, parameters);
        }

        /// <summary>
        /// 追加原始片断与参数，不处理任何占位，直接通过参数绑定
        /// </summary>
        /// <param name="query"></param>
        /// <param name="snippet">不使用{n}占位的SQL片断，如：UserAccount = @account</param>
        /// <param name="parameters">绑定原始片断中的参数，顺序应与原始片断参数位置一致</param>
        /// <returns></returns>
        public static ISqlQuery AppendRaw(this ISqlQuery query, string snippet, IEnumerable<SqlQueryParameter> parameters)
        {
            IEnumerable<SqlQueryParameter> args = parameters;
            return query.AppendRaw(snippet, args);
        }
        /// <summary>
        /// 追加原始片断与参数，不处理任何占位，直接通过参数绑定
        /// </summary>
        /// <param name="query"></param>
        /// <param name="snippet">不使用{n}占位的SQL片断，如：UserAccount = @account</param>
        /// <param name="parameters">绑定原始片断中的参数，顺序应与原始片断参数位置一致</param>
        /// <returns></returns>
        public static ISqlQuery AppendRaw(this ISqlQuery query, string snippet, params SqlQueryParameter[] parameters)
        {
            return query.AppendRaw(snippet, parameters);
        }

        /// <summary>
        /// 追加原始片断与参数并换行，不处理任何占位，直接通过参数绑定
        /// </summary>
        /// <param name="query"></param>
        /// <param name="snippet">不使用{n}占位的SQL片断，如：UserAccount = @account</param>
        /// <param name="parameters">绑定原始片断中的参数，顺序应与原始片断参数位置一致</param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLineRaw(this ISqlQuery query, string snippet, int offset, int length, SqlQueryParameter[] parameters)
        {
            return query.AppendRaw(snippet, offset, length, parameters).AppendLine();
        }

        /// <summary>
        /// 追加原始片断与参数并换行，不处理任何占位，直接通过参数绑定
        /// </summary>
        /// <param name="query"></param>
        /// <param name="snippet">不使用{n}占位的SQL片断，如：UserAccount = @account</param>
        /// <param name="parameters">绑定原始片断中的参数，顺序应与原始片断参数位置一致</param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLineRaw(this ISqlQuery query, string snippet, int offset, int length, IEnumerable<SqlQueryParameter> parameters)
        {
            return query.AppendRaw(snippet, offset, length, parameters).AppendLine();
        }

        /// <summary>
        /// 追加原始片断与参数并换行，不处理任何占位，直接通过参数绑定
        /// </summary>
        /// <param name="query"></param>
        /// <param name="snippet">不使用{n}占位的SQL片断，如：UserAccount = @account</param>
        /// <param name="parameters">绑定原始片断中的参数，顺序应与原始片断参数位置一致</param>
        /// <returns></returns>
        public static ISqlQuery AppendLineRaw(this ISqlQuery query, string snippet, params SqlQueryParameter[] parameters)
        {
            return query.AppendRaw(snippet, parameters).AppendLine();
        }

        /// <summary>
        /// 添加子查询片断
        /// </summary>
        /// <param name="query"></param>
        /// <param name="subQuery"></param>
        /// <returns></returns>
        public static ISqlQuery Append(this ISqlQuery query, ISqlQuery subQuery)
        {
            return query.AppendRaw(subQuery.GetSnippets(), subQuery.GetParameters());
        }

        /// <summary>
        /// 添加子查询片断并换行
        /// </summary>
        /// <param name="query"></param>
        /// <param name="subQuery"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLine(this ISqlQuery query, ISqlQuery subQuery)
        {
            return query.Append(subQuery).AppendLine();
        }

        /// <summary>
        /// 追加原始数字字符串
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static ISqlQuery Append(this ISqlQuery query, int val)
        {
            return query.AppendRaw(val.ToString());
        }

        /// <summary>
        /// 追加原始数字字符串并换行
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLine(this ISqlQuery query, int val)
        {
            return query.AppendRaw(val.ToString()).AppendLine();
        }

        /// <summary>
        /// 追加换行
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLine(this ISqlQuery query)
        {
            return query.Append(Environment.NewLine);
        }

        /// <summary>
        /// 追加原始SQL片断并换行
        /// </summary>
        /// <param name="query"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLine(this ISqlQuery query, string snippet)
        {
            return query.Append(snippet).AppendLine();
        }

        /// <summary>
        /// 追加原始SQL片断并换行
        /// </summary>
        /// <param name="query"></param>
        /// <param name="snippet"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static ISqlQuery AppendStringLine(this ISqlQuery query, string snippet, int offset, int length)
        {
            return query.AppendString(snippet, offset, length).AppendLine();
        }

        /// <summary>
        /// 追加片断并换行
        /// </summary>
        /// <param name="query"></param>
        /// <param name="snippet">片断中可以使用 {0}, {1} 作为参数占位</param>
        /// <param name="args">片断中占位将被替换的参数值</param>
        /// <returns></returns>
        public static ISqlQuery AppendLine(this ISqlQuery query, string snippet, params object[] args)
        {
            return query.Append(snippet, args).AppendLine();
        }

        /// <summary>
        /// 按条件追加原始SQL片断
        /// </summary>
        /// <param name="query"></param>
        /// <param name="predicate">条件值为真是，追加片断内容</param>
        /// <param name="snippet">要追加的片断内容</param>
        /// <returns></returns>
        public static ISqlQuery AppendIf(this ISqlQuery query, bool predicate, string snippet)
        {
            return predicate ? query.Append(snippet) : query;
        }

        /// <summary>
        /// 按条件追加原始SQL片断
        /// </summary>
        /// <param name="query"></param>
        /// <param name="predicate">条件值为真是，追加片断内容</param>
        /// <param name="snippet">要追加的片断内容</param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static ISqlQuery AppendStringIf(this ISqlQuery query, bool predicate, string snippet, int offset, int length)
        {
            return predicate ? query.AppendString(snippet, offset, length) : query;
        }

        /// <summary>
        /// 按条件追加片断
        /// </summary>
        /// <param name="query"></param>
        /// <param name="predicate">条件值为真是，追加片断内容</param>
        /// <param name="snippet">片断中可以使用 {0}, {1} 作为参数占位</param>
        /// <param name="args">片断中占位将被替换的参数值</param>
        /// <returns></returns>
        public static ISqlQuery AppendIf(this ISqlQuery query, bool predicate, string snippet, params object[] args)
        {
            return predicate ? query.Append(snippet, args) : query;
        }

        /// <summary>
        /// 按条件追加原始SQL片断并换行
        /// </summary>
        /// <param name="query"></param>
        /// <param name="predicate">条件值为真是，追加片断内容</param>
        /// <param name="snippet">要追加的片断内容</param>
        /// <returns></returns>
        public static ISqlQuery AppendLineIf(this ISqlQuery query, bool predicate, string snippet)
        {
            return predicate ? query.AppendLine(snippet) : query;
        }

        /// <summary>
        /// 按条件追加原始SQL片断并换行
        /// </summary>
        /// <param name="query"></param>
        /// <param name="predicate">条件值为真是，追加片断内容</param>
        /// <param name="snippet">要追加的片断内容</param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static ISqlQuery AppendStringLineIf(this ISqlQuery query, bool predicate, string snippet, int offset, int length)
        {
            return predicate ? query.AppendStringLine(snippet, offset, length) : query;
        }

        /// <summary>
        /// 按条件追加片断并换行
        /// </summary>
        /// <param name="query"></param>
        /// <param name="predicate">条件值为真是，追加片断内容</param>
        /// <param name="snippet">片断中可以使用 {0}, {1} 作为参数占位</param>
        /// <param name="args">片断中占位将被替换的参数值</param>
        /// <returns></returns>
        public static ISqlQuery AppendLineIf(this ISqlQuery query, bool predicate, string snippet, params object[] args)
        {
            return predicate ? query.AppendLine(snippet, args) : query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendIfNotNullOrEmpty(this ISqlQuery query, string val, string snippet)
        {
            return !string.IsNullOrEmpty(val) ? query.Append(snippet) : query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val"></param>
        /// <param name="snippet"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ISqlQuery AppendIfNotNullOrEmpty(this ISqlQuery query, string val, string snippet, params object[] args)
        {
            return !string.IsNullOrEmpty(val) ? query.Append(snippet, args) : query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLineIfNotNullOrEmpty(this ISqlQuery query, string val, string snippet)
        {
            return !string.IsNullOrEmpty(val) ? query.AppendLine(snippet) : query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val"></param>
        /// <param name="snippet"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLineIfNotNullOrEmpty(this ISqlQuery query, string val, string snippet, params object[] args)
        {
            return !string.IsNullOrEmpty(val) ? query.AppendLine(snippet, args) : query;
        }

        /// <summary>
        /// 当val不为null时，追加snippet片断，不支持参数占位解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="val">判定的值</param>
        /// <param name="snippet">SQL片断，支持 {n} 解析 args 位置的参数</param>
        /// <param name="args">参数清单</param>
        /// <returns></returns>
        public static ISqlQuery AppendIfNotNull<T>(this ISqlQuery query, T val, string snippet, params object[] args) where T : class
        {
            return val != null ? query.Append(snippet, args) : query;
        }

        /// <summary>
        /// 当val有值时，追加snippet片断，不支持参数占位解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="val">判定的值</param>
        /// <param name="snippet">SQL片断，支持 {n} 解析 args 位置的参数</param>
        /// <param name="args">参数清单</param>
        /// <returns></returns>
        public static ISqlQuery AppendIfNotNull<T>(this ISqlQuery query, T? val, string snippet, params object[] args) where T : struct
        {
            return val.HasValue ? query.Append(snippet, args) : query;
        }

        /// <summary>
        /// 当val不为null时，追加snippet片断，不支持参数占位解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="val">判定的值</param>
        /// <param name="snippet">SQL片断，不支持 {n} 参数解析</param>
        /// <returns></returns>
        public static ISqlQuery AppendIfNotNull<T>(this ISqlQuery query, T val, string snippet) where T : class
        {
            return val != null ? query.Append(snippet) : query;
        }

        /// <summary>
        /// 当val有值时，追加snippet片断，不支持参数占位解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="val">判定的值</param>
        /// <param name="snippet">SQL片断，不支持 {n} 参数解析</param>
        /// <returns></returns>
        public static ISqlQuery AppendIfNotNull<T>(this ISqlQuery query, T? val, string snippet) where T : struct
        {
            return val.HasValue ? query.Append(snippet) : query;
        }

        /// <summary>
        /// 当val不为null时，追加snippet片断并换行，不支持参数占位解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="val">判定的值</param>
        /// <param name="snippet">SQL片断，支持 {n} 解析 args 位置的参数</param>
        /// <param name="args">参数清单</param>
        /// <returns></returns>
        public static ISqlQuery AppendLineIfNotNull<T>(this ISqlQuery query, T val, string snippet, params object[] args) where T : class
        {
            return val != null ? query.AppendLine(snippet, args) : query;
        }

        /// <summary>
        /// 当val有值时，追加snippet片断并换行，不支持参数占位解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="val">判定的值</param>
        /// <param name="snippet">SQL片断，支持 {n} 解析 args 位置的参数</param>
        /// <param name="args">参数清单</param>
        /// <returns></returns>
        public static ISqlQuery AppendLineIfNotNull<T>(this ISqlQuery query, T? val, string snippet, params object[] args) where T : struct
        {
            return val.HasValue ? query.AppendLine(snippet, args) : query;
        }

        /// <summary>
        /// 当val不为null时，追加snippet片断并换行，不支持参数占位解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="val">判定的值</param>
        /// <param name="snippet">SQL片断，不支持 {n} 参数解析</param>
        /// <returns></returns>
        public static ISqlQuery AppendLineIfNotNull<T>(this ISqlQuery query, T val, string snippet) where T : class
        {
            return val != null ? query.AppendLine(snippet) : query;
        }

        /// <summary>
        /// 当val有值时，追加snippet片断并换行，不支持参数占位解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="val">判定的值</param>
        /// <param name="snippet">SQL片断，不支持 {n} 参数解析</param>
        /// <returns></returns>
        public static ISqlQuery AppendLineIfNotNull<T>(this ISqlQuery query, T? val, string snippet) where T : struct
        {
            return val.HasValue ? query.AppendLine(snippet) : query;
        }

        /// <summary>
        /// 当给定的 val 非空时添加片断，可将 val 作为 {0}的占用
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val">要判定的值（{0}占位值）</param>
        /// <param name="snippet">SQL片断，可使用 {0} 接收 val 参数绑定</param>
        /// <returns></returns>
        public static ISqlQuery AppendNotNull(this ISqlQuery query, object val, string snippet)
        {
            return val != null ? query.Append(snippet, val) : query;
        }

        /// <summary>
        /// 当给定的 val 非空时添加片断并换行，可将 val 作为 {0}的占用
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val">要判定的值（{0}占位值）</param>
        /// <param name="snippet">SQL片断，可使用 {0} 接收 val 参数绑定</param>
        /// <returns></returns>
        public static ISqlQuery AppendLineNotNull(this ISqlQuery query, object val, string snippet)
        {
            return val != null ? query.AppendLine(snippet, val) : query;
        }

        /// <summary>
        /// 当给定的 str 非空时添加片断，可将 str 作为 {0}的占用
        /// </summary>
        /// <param name="query"></param>
        /// <param name="str">要判定的字符串（{0}占位值）</param>
        /// <param name="snippet">SQL片断，可使用 {0} 接收 str 参数绑定</param>
        /// <returns></returns>
        public static ISqlQuery AppendNotNullOrEmpty(this ISqlQuery query, string str, string snippet)
        {
            return !string.IsNullOrEmpty(str) ? query.Append(snippet, new object[] { str }) : query;
        }

        /// <summary>
        /// 当给定的 str 非空时添加片断并换行，可将 str 作为 {0}的占用
        /// </summary>
        /// <param name="query"></param>
        /// <param name="str">要判定的字符串（{0}占位值）</param>
        /// <param name="snippet">SQL片断，可使用 {0} 接收 str 参数绑定</param>
        /// <returns></returns>
        public static ISqlQuery AppendLineNotNullOrEmpty(this ISqlQuery query, string str, string snippet)
        {
            return !string.IsNullOrEmpty(str) ? query.AppendLine(snippet, new object[] { str }) : query;
        }

        /// <summary>
        /// 添加表达式片断并换行
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLineExpression<TEntity>(this ISqlQuery query, Expression<Func<TEntity, bool>> predicate)
        where TEntity : class
        {
            return query.AppendExpression(predicate).AppendLine();
        }

        /// <summary>
        /// 添加表达式片断
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static ISqlQuery AppendExpression<TEntity>(this ISqlQuery query, Expression<Func<TEntity, bool>> predicate)
        where TEntity : class
        {
            return query.AppendExpression(new SqlExpressionVisitor<TEntity>(query.Database, Descriptors.Operation.Select), predicate);
        }

        /// <summary>
        /// 当条件 condition 为真是添加条件表达式
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition">判定条件</param>
        /// <param name="predicate">表达式</param>
        /// <returns></returns>
        public static ISqlQuery AppendExpressionIf<TEntity>(this ISqlQuery query, bool condition, Expression<Func<TEntity, bool>> predicate)
        where TEntity : class
        {
            return condition ? query.AppendExpression(predicate) : query;
        }

        /// <summary>
        /// 当条件 condition 为真是添加条件表达式并换行
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition">判定条件</param>
        /// <param name="predicate">表达式</param>
        /// <returns></returns>
        public static ISqlQuery AppendLineExpressionIf<TEntity>(this ISqlQuery query, bool condition, Expression<Func<TEntity, bool>> predicate)
        where TEntity : class
        {
            return condition ? query.AppendLineExpression(predicate) : query;
        }

        /// <summary>
        /// 添加表达式片断
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="expressionVisitor"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static ISqlQuery AppendExpression<TEntity>(this ISqlQuery query, SqlExpressionVisitor<TEntity> expressionVisitor, Expression<Func<TEntity, bool>> predicate)
        where TEntity : class
        {
            expressionVisitor.Visit(predicate);
            return query.AppendRaw("( ").AppendRaw(expressionVisitor.Sql, expressionVisitor.Parameters).AppendRaw(" )");
        }

        /// <summary>
        /// 如果 items 集合非空，追加 snippet 片断，并使用集合替换 {0}
        /// 已知：oracle 在 in 超过1000时超出限制
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query"></param>
        /// <param name="items"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendIn<TItem>(this ISqlQuery query, string snippet, IEnumerable<TItem> items)
        {
            if (items == null) return query;

            using (var enumerator = items.GetEnumerator())
            {
                // 无元素
                if (!enumerator.MoveNext()) return query;

                var parameterNames = new StringBuilder();
                var parameters = new List<SqlQueryParameter>();
                while (true)
                {
                    var name = query.NextParameterName();
                    parameters.Add(new SqlQueryParameter(name, enumerator.Current));
                    parameterNames.Append(name);
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    parameterNames.Append(", ");
                }
                return query.AppendRaw(string.Format(snippet, parameterNames), parameters);
            }
        }

        /// <summary>
        /// 如果 items 集合非空，追加 snippet 片断，并使用集合替换 {0}
        /// 已知：oracle 在 in 超过1000时超出限制
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query"></param>
        /// <param name="set"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendIn<TItem>(this ISqlQuery query, string snippet, HashSet<TItem> set)
        {
            return query.AppendIn(snippet, (IEnumerable<TItem>)set);
        }

        /// <summary>
        /// 如果 items 集合非空，追加 snippet 片断，并使用集合替换 {0}
        /// 已知：oracle 在 in 超过1000时超出限制
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query"></param>
        /// <param name="items"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendIn<TItem>(this ISqlQuery query, string snippet, params TItem[] items)
        {
            return query.AppendIn(snippet, (IEnumerable<TItem>)items);
        }

        /// <summary>
        /// 如果 items 集合非空，追加 snippet 片断，并使用集合替换 {0}
        /// 已知：oracle 在 in 超过1000时超出限制
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query"></param>
        /// <param name="set"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLineIn<TItem>(this ISqlQuery query, string snippet, HashSet<TItem> set)
        {
            return query.AppendIn(snippet, (IEnumerable<TItem>)set);
        }

        /// <summary>
        /// 如果 items 集合非空，追加 snippet 片断，并使用集合替换 {0}
        /// 已知：oracle 在 in 超过1000时超出限制
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query"></param>
        /// <param name="items"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendLineIn<TItem>(this ISqlQuery query, string snippet, params TItem[] items)
        {
            return query.AppendIn(snippet, items).AppendLine();
        }
        #endregion

        #region Append****Like***
        /// <summary>
        /// 字符串非空时，追加snippet片断，并使用{0}值为 val + '%'
        ///     等价   AppendIf(!string.IsNullOrEmpty(val), snippet, val + '%');
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendStartsWithNotNullOrEmpty(this ISqlQuery query, string val, string snippet)
        {
            return string.IsNullOrEmpty(val) ? query : query.Append(snippet, val + '%');
        }

        /// <summary>
        /// 字符串非空时，追加snippet片断，并使用{0}值为 '%' + val
        ///     等价   AppendIf(!string.IsNullOrEmpty(val), snippet, %' + val);
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendEndsWithNotNullOrEmpty(this ISqlQuery query, string val, string snippet)
        {
            return string.IsNullOrEmpty(val) ? query : query.Append(snippet, '%' + val);
        }

        /// <summary>
        /// 字符串非空时，追加snippet片断，并使用{0}值为 '%' + val + '%'
        ///     等价   AppendIf(!string.IsNullOrEmpty(val), snippet, %' + val + '%');
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendContainsNotNullOrEmpty(this ISqlQuery query, string val, string snippet)
        {
            return string.IsNullOrEmpty(val) ? query : query.Append(snippet, '%' + val + '%');
        }
        /// <summary>
        /// 字符串非空时，追加snippet片断并换行，并使用{0}值为 val + '%'
        ///     等价   AppendIf(!string.IsNullOrEmpty(val), snippet, val + '%');
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendStartsWithLineNotNullOrEmpty(this ISqlQuery query, string val, string snippet)
        {
            return string.IsNullOrEmpty(val) ? query : query.AppendLine(snippet, val + '%');
        }

        /// <summary>
        /// 字符串非空时，追加snippet片断并换行，并使用{0}值为 '%' + val
        ///     等价   AppendIf(!string.IsNullOrEmpty(val), snippet, %' + val);
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendEndsWithLineNotNullOrEmpty(this ISqlQuery query, string val, string snippet)
        {
            return string.IsNullOrEmpty(val) ? query : query.AppendLine(snippet, '%' + val);
        }

        /// <summary>
        /// 字符串非空时，追加snippet片断并换行，并使用{0}值为 '%' + val + '%'
        ///     等价   AppendIf(!string.IsNullOrEmpty(val), snippet, %' + val + '%');
        /// </summary>
        /// <param name="query"></param>
        /// <param name="val"></param>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static ISqlQuery AppendContainsLineNotNullOrEmpty(this ISqlQuery query, string val, string snippet)
        {
            return string.IsNullOrEmpty(val) ? query : query.AppendLine(snippet, '%' + val + '%');
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
        /// <example>db.ExecuteSqlScalar("SELECT * FROM example WHERE Name LIKE {0} +'%'", "ad")</example>
        /// <param name="database"></param>
        /// <param name="sql">SQL语句，可使用 {0} 作为参数占位引用后续参数值</param>
        /// <param name="args">参数清单</param>
        /// <returns></returns>
        static public T ExecuteSqlScalar<T>(this IDatabase database, string sql, params object[] args)
        {
            return database.SqlQuery(sql, args).ExecuteScalar<T>();
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
        static public TEntity ExecuteSqlEntity<TEntity>(this IDatabase database, string sql, params object[] args)
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
        static public TEntity[] ExecuteSqlEntities<TEntity>(this IDatabase database, string sql, params object[] args)
        {
            if (args == null || args.Length == 0)
                return database.ExecuteEntities<TEntity>(sql);

            return database.SqlQuery(sql, args).ExecuteEntities<TEntity>();
        }

        /// <summary>
        /// 执行指定SQL，并传入给定参数
        /// </summary>
        /// <example>db.ExecuteSqlEntities("SELECT * FROM example WHERE Name LIKE {0} +'%'", "ad")</example>
        /// <param name="database"></param>
        /// <param name="sql">SQL语句，可使用 {0} 作为参数占位引用后续参数值</param>
        /// <param name="args">命名参数如:   new { name = "OK", code = "ok" }</param>
        /// <returns></returns>
        static public IEnumerable<TEntity> ExecuteSqlEnumerable<TEntity>(this IDatabase database, string sql, params object[] args)
        {
            return database.SqlQuery(sql, args).ExecuteEnumerable<TEntity>();
        }
        #endregion

        #region Update
        /// <summary>
        /// 扩展 Update 实现
        ///     db.Update&lt;TEntity&gt;().Set(new {Deleted = true, ModifyTime = DateTime.Now}).Where(_ => _.KeyId == 5);
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="database"></param>
        /// <returns></returns>
        static public UpdateBuilder<TEntity> Update<TEntity>(this IDatabase database) where TEntity : class, new()
        {
            return new UpdateBuilder<TEntity>(database);
        }
        #endregion
    }
}
