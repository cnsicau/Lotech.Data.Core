using System.Linq.Expressions;

namespace Lotech.Data.Operations.Visitors
{
    class NotVisitor<TEntity> : IExpressionNodeVisitor<TEntity, UnaryExpression> where TEntity : class
    {
        void IExpressionNodeVisitor<TEntity, UnaryExpression>.Visit(SqlExpressionVisitor<TEntity> visitor, UnaryExpression node)
        {
            if (node.Operand.NodeType == ExpressionType.MemberAccess
                || node.Operand.NodeType == ExpressionType.Convert
                || node.Operand.NodeType == ExpressionType.ConvertChecked
                || node.Operand.NodeType == ExpressionType.Call && ((MethodCallExpression)node.Operand).Method.ReturnType != typeof(bool))
            {
                visitor.AddFragment("1 <> ");
                visitor.Visit(node.Operand);
            }
            else
            {
                visitor.AddFragment("NOT (");
                visitor.Visit(node.Operand);
                visitor.AddFragment(")");
            }
        }
    }
}
