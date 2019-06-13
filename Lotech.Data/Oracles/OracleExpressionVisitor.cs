using Lotech.Data.Descriptors;
using Lotech.Data.Operations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lotech.Data.Oracles
{
    class OracleExpressionVisitor<TEntity> : SqlExpressionVisitor<TEntity> where TEntity : class
    {
        // 函数调用映射
        static readonly Dictionary<MethodInfo, Action<SqlExpressionVisitor<TEntity>, MethodCallExpression>>
            methodCallVisitors = new Dictionary<MethodInfo, Action<SqlExpressionVisitor<TEntity>, MethodCallExpression>>
            {
                {Methods.ToUpper, VisitToUpper },
                {Methods.ToLower, VisitToLower },
                {Methods.StartsWith, VisitStartsWith },
                {Methods.EndsWith, VisitEndsWith },
                {Methods.Contains, VisitContains },
                {Methods.Substring, VisitSubstring },
                {Methods.SubstringLength, VisitSubstring },
                {Methods.IsNullOrEmpty, VisitIsNullOrEmpty },
            };

        static void VisitIsNullOrEmpty(SqlExpressionVisitor<TEntity> visitor, MethodCallExpression call)
        {
            visitor.Visit(call.Object);
            visitor.AddFragment(" IS NULL");
        }

        #region Method Visitors
        static void VisitToUpper(SqlExpressionVisitor<TEntity> visitor, MethodCallExpression call)
        {
            visitor.AddFragment("UPPER(");
            visitor.Visit(call.Object);
            visitor.AddFragment(")");
        }
        static void VisitToLower(SqlExpressionVisitor<TEntity> visitor, MethodCallExpression call)
        {
            visitor.AddFragment("LOWER(");
            visitor.Visit(call.Object);
            visitor.AddFragment(")");
        }
        static void VisitStartsWith(SqlExpressionVisitor<TEntity> visitor, MethodCallExpression call)
        {
            visitor.Visit(call.Object);
            visitor.AddFragment(" LIKE ");
            visitor.Visit(call.Arguments[0]);
            visitor.AddFragment(" || '%'");
        }
        static void VisitEndsWith(SqlExpressionVisitor<TEntity> visitor, MethodCallExpression call)
        {
            visitor.Visit(call.Object);
            visitor.AddFragment(" LIKE '%' || ");
            visitor.Visit(call.Arguments[0]);
        }
        static void VisitContains(SqlExpressionVisitor<TEntity> visitor, MethodCallExpression call)
        {
            visitor.Visit(call.Object);
            visitor.AddFragment(" LIKE '%' || ");
            visitor.Visit(call.Arguments[0]);
            visitor.AddFragment(" || '%'");
        }
        static void VisitSubstring(SqlExpressionVisitor<TEntity> visitor, MethodCallExpression call)
        {
            visitor.AddFragment("SUBSTR(");
            visitor.Visit(call.Object);
            foreach (var arg in call.Arguments)
            {
                visitor.AddFragment(", ");
                visitor.Visit(arg);
            }
            visitor.AddFragment(")");
        }
        #endregion

        public OracleExpressionVisitor(IDatabase database, Operation operation)
            : base(database, operation) { }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Action<SqlExpressionVisitor<TEntity>, MethodCallExpression> visitor;
            if (methodCallVisitors.TryGetValue(node.Method, out visitor))
            {
                visitor(this, node);
            }
            else if (node.Method.Name == "ToString" && node.Arguments.Count == 0)
            {
                if (node.Method.DeclaringType == typeof(string)) // 忽略 string.ToString转换
                {
                    Visit(node.Object);
                }
                else
                {
                    AddFragment("'' || ");
                    Visit(node.Object);
                }
            }
            else
            {
                return base.VisitMethodCall(node);
            }
            return null;
        }
        protected override Expression VisitContainsCall(MethodCallExpression call, Expression element, Expression collection)
        {
            var valueVisitor = new OracleExpressionVisitor<TEntity>(Database, Operation);
            valueVisitor.Visit(element);
            var field = valueVisitor.Sql;
            valueVisitor.Visit(collection);
            var values = (valueVisitor.Parameters.LastOrDefault().Value as IEnumerable)?.GetEnumerator();
            {
                if (values == null || !values.MoveNext())
                {
                    AddFragment(field);
                    AddFragment(" IS NULL");
                }
                else
                {
                    AddFragment("(");
                    var elementType = element.Type;
                    var next = true;
                    while (next)
                    {
                        AddFragment(field);
                        AddFragment(" = ");
                        AddParameter(elementType, values.Current);
                        if (next = values.MoveNext())
                            AddFragment(" OR ");
                    }
                    AddFragment(")");
                }
            }

            if (values is IDisposable) ((IDisposable)values).Dispose();
            return null;
        }
    }
}
