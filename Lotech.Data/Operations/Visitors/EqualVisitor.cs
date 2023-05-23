using System.Linq.Expressions;

namespace Lotech.Data.Operations.Visitors
{
    class EqualVisitor<TEntity> : IExpressionNodeVisitor<TEntity, BinaryExpression> where TEntity : class
    {
        void IExpressionNodeVisitor<TEntity, BinaryExpression>.Visit(SqlExpressionVisitor<TEntity> visitor, BinaryExpression node)
        {
            if (node.Left.NodeType == ExpressionType.Constant && ((ConstantExpression)node.Left).Value == null)
            {
                visitor.Visit(node.Right);
                visitor.AddFragment(" IS NULL");
            }
            else if (node.Right.NodeType == ExpressionType.Constant && ((ConstantExpression)node.Right).Value == null)
            {
                visitor.Visit(node.Left);
                visitor.AddFragment(" IS NULL");
            }
            else
            {
                visitor.Visit(node.Left);
                visitor.AddFragment(" = ");
                visitor.Visit(node.Right);
            }
        }
    }
}
