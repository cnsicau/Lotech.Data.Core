using System.Linq.Expressions;

namespace Lotech.Data.Operations.Visitors
{
    class ConvertVisitor<TEntity> : IExpressionNodeVisitor<TEntity, UnaryExpression> where TEntity : class
    {
        void IExpressionNodeVisitor<TEntity, UnaryExpression>.Visit(SqlExpressionVisitor<TEntity> visitor, UnaryExpression node)
        {
            visitor.Visit(node.Operand);    // 忽略 CONVERT 直接访问内部操作数
        }
    }
}
