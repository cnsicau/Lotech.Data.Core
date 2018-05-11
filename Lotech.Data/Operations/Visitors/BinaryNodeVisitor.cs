using System.Linq.Expressions;

namespace Lotech.Data.Operations.Visitors
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class BinaryNodeVisitor<TEntity> : IExpressionNodeVisitor<TEntity, BinaryExpression> where TEntity : class
    {
        #region Static Fileds
        /// <summary>
        /// 
        /// </summary>
        static public IExpressionNodeVisitor<TEntity, BinaryExpression> And = new BinaryNodeVisitor<TEntity>("AND");
        /// <summary>
        /// 
        /// </summary>
        static public IExpressionNodeVisitor<TEntity, BinaryExpression> Or = new BinaryNodeVisitor<TEntity>("OR");
        /// <summary>
        /// 
        /// </summary>
        static public IExpressionNodeVisitor<TEntity, BinaryExpression> Greater = new BinaryNodeVisitor<TEntity>(">");
        /// <summary>
        /// 
        /// </summary>
        static public IExpressionNodeVisitor<TEntity, BinaryExpression> GreaterEqual = new BinaryNodeVisitor<TEntity>(">=");
        /// <summary>
        /// 
        /// </summary>
        static public IExpressionNodeVisitor<TEntity, BinaryExpression> Less = new BinaryNodeVisitor<TEntity>("<");
        /// <summary>
        /// 
        /// </summary>
        static public IExpressionNodeVisitor<TEntity, BinaryExpression> LessEqual = new BinaryNodeVisitor<TEntity>("<=");
        #endregion
        private readonly string _operator;

        /// <summary>
        /// 
        /// </summary>
        protected BinaryNodeVisitor() { }
        /// <summary>
        /// 
        /// </summary>
        public BinaryNodeVisitor(string @operator)
        {
            this._operator = @operator;
        }

        void IExpressionNodeVisitor<TEntity, BinaryExpression>.Visit(SqlExpressionVisitor<TEntity> visitor, BinaryExpression node)
        {
            visitor.Visit(node.Left);
            visitor.AddFragment(" ");
            visitor.AddFragment(_operator);
            visitor.AddFragment(" ");
            visitor.Visit(node.Right);
        }
    }
}
