using Lotech.Data.Descriptors;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Lotech.Data.Utils;

namespace Lotech.Data.Operations.Visitors
{
    class MemberVisitor<TEntity> : IExpressionNodeVisitor<TEntity, MemberExpression> where TEntity : class
    {
        private readonly EntityDescriptor _descriptor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptor"></param>
        public MemberVisitor(EntityDescriptor descriptor)
        {
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        void IExpressionNodeVisitor<TEntity, MemberExpression>.Visit(SqlExpressionVisitor<TEntity> visitor, MemberExpression node)
        {
            if (node.Expression?.NodeType == ExpressionType.Parameter)
            {
                if (node.Expression.Type != typeof(TEntity))
                    throw new InvalidOperationException($"仅支持访问模型{typeof(TEntity).Name}参数的成员 {node}.");

                var member = _descriptor.Members?.FirstOrDefault(_ => _.Member == node.Member);
                if (member == null)
                    throw new InvalidOperationException($"访问模型{typeof(TEntity).Name}参数未找到映射 {node}.");

                visitor.AddName(member.Name);
            }
            else
            {
                visitor.Visit(EvaluateExternalMember(node));
            }
        }

        ConstantExpression EvaluateExternalMember(MemberExpression node)
        {
            if (node.Expression == null) // static member
            {
                return Expression.Constant(MemberAccessor.GetGetter(node.Member)(null));
            }
            switch (node.Expression.NodeType)
            {
                case ExpressionType.Constant:
                    return Expression.Constant(MemberAccessor.GetGetter(node.Member)(((ConstantExpression)node.Expression).Value));
                case ExpressionType.MemberAccess:
                    var value = EvaluateExternalMember((MemberExpression)node.Expression);  // 递归向内获取
                    return Expression.Constant(MemberAccessor.GetGetter(node.Member)(value.Value));
                default:
                    throw new NotSupportedException("不支持的外部值访问：" + node);
            }
        }
    }
}
