using Lotech.Data.Descriptors;
using Lotech.Data.Operations.Visitors;
using Lotech.Data.Queries;
using Lotech.Data.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Lotech.Data.Operations
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlExpressionVisitor<EntityType> : ExpressionVisitor
        where EntityType : class
    {
        #region Methods
        /// <summary>
        /// 预定义方法调用
        /// </summary>
        protected static class Methods
        {
            /// <summary>
            /// String.ToUpper()
            /// </summary>
            public static readonly MethodInfo ToUpper = new Func<string>(string.Empty.ToUpper).Method;
            /// <summary>
            /// String.ToLower()
            /// </summary>
            public static readonly MethodInfo ToLower = new Func<string>(string.Empty.ToLower).Method;
            /// <summary>
            /// String.StartsWith(string)
            /// </summary>
            public static readonly MethodInfo StartsWith = new Func<string, bool>(string.Empty.StartsWith).Method;

            /// <summary>
            /// String.EndsWith(string)
            /// </summary>
            public static readonly MethodInfo EndsWith = new Func<string, bool>(string.Empty.EndsWith).Method;

            /// <summary>
            /// String.Contains(string)
            /// </summary>
            public static readonly MethodInfo Contains = new Func<string, bool>(string.Empty.Contains).Method;
            /// <summary>
            /// String.Substring(int position)
            /// </summary>
            public static readonly MethodInfo Substring = new Func<int, string>(string.Empty.Substring).Method;
            /// <summary>
            /// String.Contains(int position, int length)
            /// </summary>
            public static readonly MethodInfo SubstringLength = new Func<int, int, string>(string.Empty.Substring).Method;
        }
        #endregion

        /// <summary>
        /// 全局参数ID编号，以避免构建子查询时参数名可能重复问题
        /// </summary>
        [ThreadStatic]
        private static ushort global_id = 0;
        private readonly IDatabase _database;
        private readonly Operation _operation;
        private readonly IEntityDescriptor _descriptor;
        private readonly StringBuilder _sql = new StringBuilder();
        private readonly List<SqlQueryParameter> _parameters = new List<SqlQueryParameter>();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="operation"></param>
        public SqlExpressionVisitor(IDatabase database, Operation operation) : this(database, database.DescriptorProvider.GetEntityDescriptor<EntityType>(operation), operation) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="descriptor"></param>
        /// <param name="operation"></param>
        SqlExpressionVisitor(IDatabase database, IEntityDescriptor descriptor, Operation operation)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            _database = database;

            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            _descriptor = descriptor;
            _operation = operation;
            global_id++;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<SqlQueryParameter> Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Sql { get { return _sql.ToString(); } }

        /// <summary>
        /// 
        /// </summary>
        protected IDatabase Database { get { return _database; } }

        /// <summary>
        /// 
        /// </summary>
        protected Operation Operation { get { return _operation; } }

        /// <summary>
        /// 追加SQL片断
        /// </summary>
        /// <param name="fragment"></param>
        public virtual void AddFragment(string fragment) { _sql.Append(fragment); }

        /// <summary>
        /// 添加名称引用
        /// </summary>
        /// <param name="member"></param>
        public virtual void AddName(string member) { _sql.Append(_database.QuoteName(member)); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterType"></param>
        /// <param name="parameterValue"></param>
        public virtual void AddParameter(Type parameterType, object parameterValue)
        {
            var name = _database.BuildParameterName("xp" + global_id + "_" + _parameters.Count);
            _sql.Append(name);
            _parameters.Add(new SqlQueryParameter(name, parameterType, parameterValue));
        }

        /// <summary>
        /// 创建Command并绑定参数
        /// </summary>
        public virtual DbCommand CreateCommand(string sql, Expression<Func<EntityType, bool>> predicate)
        {
            AddFragment(sql);
            Visit(predicate);
            var command = Database.GetSqlStringCommand(Sql);

            foreach (var p in Parameters)
            {
                Database.AddInParameter(command, p.Name, _database.ParseDbType(p.Type), p.Value);
            }
            return command;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            IExpressionNodeVisitor<EntityType, MemberExpression> visitor = new MemberVisitor<EntityType>(_descriptor);
            visitor.Visit(this, node);
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            new ConstantVisitor<EntityType>().Visit(this, node);
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            IExpressionNodeVisitor<EntityType, BinaryExpression> visitor;
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                    visitor = BinaryNodeVisitor<EntityType>.And;
                    break;
                case ExpressionType.OrElse:
                    visitor = BinaryNodeVisitor<EntityType>.Or;
                    break;
                case ExpressionType.Equal:
                    visitor = new EqualVisitor<EntityType>();
                    break;
                case ExpressionType.NotEqual:
                    visitor = new NotEqualVisitor<EntityType>();
                    break;
                case ExpressionType.GreaterThan:
                    visitor = BinaryNodeVisitor<EntityType>.Greater;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    visitor = BinaryNodeVisitor<EntityType>.GreaterEqual;
                    break;
                case ExpressionType.LessThan:
                    visitor = BinaryNodeVisitor<EntityType>.Less;
                    break;
                case ExpressionType.LessThanOrEqual:
                    visitor = BinaryNodeVisitor<EntityType>.LessEqual;
                    break;
                default:
                    throw new NotSupportedException();
            }
            visitor.Visit(this, node);
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method;
            if (method.Name == nameof(List<EntityType>.Contains))
            {
                if (!method.IsStatic && method.DeclaringType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    return VisitContainsCall(node, node.Arguments[0], node.Object);
                }
                else if (method.IsStatic && method.DeclaringType == typeof(System.Linq.Enumerable))
                {
                    return VisitContainsCall(node, node.Arguments[1], node.Arguments[0]);
                }
            }
            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// 访问 Contains 调用, 用于生成 IN 调用
        /// </summary>
        /// <param name="call"></param>
        /// <param name="element"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        protected virtual Expression VisitContainsCall(MethodCallExpression call, Expression element, Expression collection)
        {
            Visit(element);

            var valueVisitor = new SqlExpressionVisitor<EntityType>(Database, Operation);
            valueVisitor.Visit(collection);
            var values = (valueVisitor.Parameters.FirstOrDefault().Value as System.Collections.IEnumerable)?.GetEnumerator();
            {
                AddFragment(" IN (");
                if (values == null || !values.MoveNext()) AddFragment("NULL");
                else
                {
                    var elementType = element.Type;
                    var next = true;
                    while (next)
                    {
                        AddParameter(elementType, values.Current);
                        if (next = values.MoveNext())
                            AddFragment(", ");
                    }
                }
                AddFragment(")");
            }

            if (values is IDisposable) ((IDisposable)values).Dispose();

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            IExpressionNodeVisitor<EntityType, UnaryExpression> visitor;
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    visitor = new NotVisitor<EntityType>();
                    break;
                case ExpressionType.Convert:
                    visitor = new ConvertVisitor<EntityType>();
                    break;
                default:
                    throw new NotSupportedException();
            }

            visitor.Visit(this, node);
            return null;
        }

        #region Not Supported Nodes
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitBlock(BlockExpression node)
        {
            throw new NotSupportedException("不支持 Block: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            throw new NotSupportedException("不支持 Catch: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            throw new NotSupportedException("不支持 DebugInfo: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (typeof(T) != typeof(Func<EntityType, bool>))
                throw new NotSupportedException("不支持 Lambda: " + node);

            Visit(node.Body);
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitExtension(Expression node)
        {
            throw new NotSupportedException("不支持 Extension: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            throw new NotSupportedException("不支持 Conditional: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            throw new NotSupportedException("不支持 TypeBinary: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitTry(TryExpression node)
        {
            throw new NotSupportedException("不支持 Try: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            throw new NotSupportedException("不支持 SwitchCase: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitSwitch(SwitchExpression node)
        {
            throw new NotSupportedException("不支持 Switch: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            throw new NotSupportedException("不支持 RuntimeVariables: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            throw new NotSupportedException("不支持 NewArray: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitNew(NewExpression node)
        {
            throw new NotSupportedException("不支持 New: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            throw new NotSupportedException("不支持 MemberMemberBinding: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            throw new NotSupportedException("不支持 MemberListBinding: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            throw new NotSupportedException("不支持 MemberInit: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            throw new NotSupportedException("不支持 MemberBinding: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            throw new NotSupportedException("不支持 MemberAssignment: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitLoop(LoopExpression node)
        {
            throw new NotSupportedException("不支持 Loop: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitListInit(ListInitExpression node)
        {
            throw new NotSupportedException("不支持 ListInit: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            throw new NotSupportedException("不支持 LabelTarget: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitGoto(GotoExpression node)
        {
            throw new NotSupportedException("不支持 Goto: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitDefault(DefaultExpression node)
        {
            throw new NotSupportedException("不支持 Default: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitDynamic(DynamicExpression node)
        {
            throw new NotSupportedException("不支持 Dynamic: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override ElementInit VisitElementInit(ElementInit node)
        {
            throw new NotSupportedException("不支持 ElementInit: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitInvocation(InvocationExpression node)
        {
            throw new NotSupportedException("不支持 Invocation: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitIndex(IndexExpression node)
        {
            throw new NotSupportedException("不支持 Index: " + node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitLabel(LabelExpression node)
        {
            throw new NotSupportedException("不支持 Label: " + node);
        }

        #endregion
    }
}
