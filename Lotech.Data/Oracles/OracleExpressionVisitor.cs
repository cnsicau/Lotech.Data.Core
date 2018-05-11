using Lotech.Data.Descriptors;
using Lotech.Data.Operations;
using System.Linq.Expressions;

namespace Lotech.Data.Oracles
{
    class OracleExpressionVisitor<TEntity> : SqlExpressionVisitor<TEntity> where TEntity : class
    {
        public OracleExpressionVisitor(IDatabase database)
       : base(database, AttributeDescriptorFactory.Create<TEntity>()) { }


        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method;
            Visit(node.Object);
            if (method == Methods.Contains)
            {
                AddFragment(" LIKE '%' || ");
                foreach (var arg in node.Arguments) Visit(arg);
                AddFragment(" || '%'");
            }
            else if (method == Methods.StartsWith)
            {
                AddFragment(" LIKE ");
                foreach (var arg in node.Arguments) Visit(arg);
                AddFragment(" || '%'");
            }
            else if (method == Methods.EndsWith)
            {
                AddFragment(" LIKE '%' || ");
                foreach (var arg in node.Arguments) Visit(arg);
            }
            else
            {
                return base.VisitMethodCall(node);
            }
            return null;
        }
    }
}
