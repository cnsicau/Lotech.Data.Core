using System.Linq.Expressions;

namespace Lotech.Data.Operations.Visitors
{
    class NotEqualVisitor<EntityType> : IExpressionNodeVisitor<EntityType, BinaryExpression> where EntityType : class
    {
        void IExpressionNodeVisitor<EntityType, BinaryExpression>.Visit(SqlExpressionVisitor<EntityType> visitor, BinaryExpression node)
        {
            if (node.Left.NodeType == ExpressionType.Constant && ((ConstantExpression)node.Left).Value == null)
            {
                visitor.Visit(node.Right);
                visitor.AddFragment(" IS NOT NULL");
            }
            else if (node.Right.NodeType == ExpressionType.Constant && ((ConstantExpression)node.Right).Value == null)
            {
                visitor.Visit(node.Left);
                visitor.AddFragment(" IS NOT NULL");
            }
            else
            {
                visitor.Visit(node.Left);
                visitor.AddFragment(" <> ");
                visitor.Visit(node.Right);
            }
        }
    }
}
