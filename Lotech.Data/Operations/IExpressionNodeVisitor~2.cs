using System.Linq.Expressions;

namespace Lotech.Data.Operations
{
    /// <summary>
    /// 节点访问者
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TExpressionNode"></typeparam>
    public interface IExpressionNodeVisitor<TEntity, TExpressionNode>
        where TEntity : class
        where TExpressionNode : Expression
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="visitor"></param>
        /// <param name="node"></param>
        void Visit(SqlExpressionVisitor<TEntity> visitor, TExpressionNode node);
    }
}
