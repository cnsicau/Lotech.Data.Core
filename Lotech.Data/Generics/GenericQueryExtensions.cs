﻿using Lotech.Data.Operations;
using System;
using System.Linq.Expressions;

namespace Lotech.Data.Generics
{
    /// <summary>
    /// 
    /// </summary>
    public static class GenericQueryExtensions
    {
        /// <summary>
        /// 通过 Lamda 表达式添加条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static ISqlQuery Append<T>(this ISqlQuery query, Expression<Func<T, bool>> predicate) where T : class
        {
            return query.AppendExpression(new SqlExpressionVisitor<T>(query.Database, Descriptors.Operation.None), predicate);
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