using System;
using System.Linq.Expressions;

namespace Lotech.Data.Operations.Visitors
{
    class NotVisitor<TEntity> : IExpressionNodeVisitor<TEntity, UnaryExpression> where TEntity : class
    {
        void IExpressionNodeVisitor<TEntity, UnaryExpression>.Visit(SqlExpressionVisitor<TEntity> visitor, UnaryExpression node)
        {
            visitor.Visit(node.Operand);
            visitor.AddFragment(" <> 0");
        }
    }
}
