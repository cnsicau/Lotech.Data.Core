using Lotech.Data.Descriptors;
using Lotech.Data.Operations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lotech.Data.SqlServers
{
    class SqlServerExpressionVisitor<TEntity> : SqlExpressionVisitor<TEntity> where TEntity : class
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
                {Methods.SubstringLength, VisitSubstring2 },
            };

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
            visitor.AddFragment(" + '%'");
        }
        static void VisitEndsWith(SqlExpressionVisitor<TEntity> visitor, MethodCallExpression call)
        {
            visitor.Visit(call.Object);
            visitor.AddFragment(" LIKE '%' + ");
            visitor.Visit(call.Arguments[0]);
        }
        static void VisitContains(SqlExpressionVisitor<TEntity> visitor, MethodCallExpression call)
        {
            visitor.Visit(call.Object);
            visitor.AddFragment(" LIKE '%' + ");
            visitor.Visit(call.Arguments[0]);
            visitor.AddFragment(" + '%'");
        }
        static void VisitSubstring(SqlExpressionVisitor<TEntity> visitor, MethodCallExpression call)
        {
            visitor.AddFragment("SUBSTRING(");
            visitor.Visit(call.Object);
            visitor.AddFragment(", ");
            visitor.Visit(call.Arguments[0]);
            visitor.AddFragment(", 8000)");
        }
        static void VisitSubstring2(SqlExpressionVisitor<TEntity> visitor, MethodCallExpression call)
        {
            visitor.AddFragment("SUBSTRING(");
            visitor.Visit(call.Object);
            visitor.AddFragment(", ");
            visitor.Visit(call.Arguments[0]);
            visitor.AddFragment(", ");
            visitor.Visit(call.Arguments[1]);
            visitor.AddFragment(")");
        }
        #endregion

        public SqlServerExpressionVisitor(IDatabase database)
            : base(database, AttributeDescriptorFactory.Create<TEntity>()) { }

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
                    AddFragment("CONVERT(NVARCHAR(4000), ");
                    Visit(node.Object);
                    AddFragment(")");
                }
            }
            // list.Contains(_.Member) ...
            else if (node.Method.Name == "Contains" && node.Method.IsGenericMethod
                    && node.Arguments.Count == 2 && node.Method.DeclaringType == typeof(Enumerable))
            {
                var collectionVisitor = new SqlServerExpressionVisitor<TEntity>(Database);
                collectionVisitor.Visit(node.Arguments[0]);
                var values = (collectionVisitor.Parameters.FirstOrDefault().Value as IEnumerable)?.GetEnumerator();

                Visit(node.Arguments[1]);
                AddFragment(" IN (");
                if (values == null || !values.MoveNext()) AddFragment("NULL");
                else
                {
                    var elementType = node.Method.GetGenericArguments().Single();
                    AddParameter(elementType, values.Current);
                    while (values.MoveNext())
                    {
                        AddFragment(", ");
                        AddParameter(elementType, values.Current);
                    }
                }
                AddFragment(")");
            }
            else
            {
                return base.VisitMethodCall(node);
            }
            return null;
        }
    }
}
