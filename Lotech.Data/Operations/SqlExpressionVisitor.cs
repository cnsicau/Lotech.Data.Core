using Lotech.Data.Descriptors;
using Lotech.Data.Operations.Visitors;
using Lotech.Data.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
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

        private readonly IDatabase _database;
        private readonly EntityDescriptor _descriptor;
        private readonly StringBuilder _sql = new StringBuilder();
        private readonly List<ExpressionParameter> _parameters = new List<ExpressionParameter>();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        public SqlExpressionVisitor(IDatabase database) : this(database, AttributeDescriptorFactory.Create<EntityType>()) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="descriptor"></param>
        public SqlExpressionVisitor(IDatabase database, EntityDescriptor descriptor)
        {
            if(database  == null) throw new ArgumentNullException(nameof(database));
            _database  = database ;

            if(descriptor  == null) throw new ArgumentNullException(nameof(descriptor));
            _descriptor  = descriptor ;

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IReadOnlyCollection<ExpressionParameter> Parameters
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
            var name = "p_sql_" + _parameters.Count;
            _sql.Append(_database.BuildParameterName(name));
            _parameters.Add(new ExpressionParameter(name, parameterType, parameterValue));
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
                Database.AddInParameter(command, p.Name, DbTypeParser.Parse(p.Type), p.Value);
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
            return base.VisitMethodCall(node);
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
