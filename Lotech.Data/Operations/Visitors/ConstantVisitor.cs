using Lotech.Data.Operations;
using System.Linq.Expressions;

namespace Lotech.Data.Operations.Visitors
{

    class ConstantVisitor<TEntity> : IExpressionNodeVisitor<TEntity, ConstantExpression> where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="visitor"></param>
        /// <param name="node"></param>
        public void Visit(SqlExpressionVisitor<TEntity> visitor, ConstantExpression node)
        {
            visitor.AddParameter(node.Type, node.Value);
        }
    }
}
