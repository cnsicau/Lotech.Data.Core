using Lotech.Data.Descriptors;
using Lotech.Data.Operations;
using Lotech.Data.Utils;
using System;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;

    /// <summary>
    /// 
    /// </summary>
    class UpdateOperationBuilder<TEntity> : IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>
        where TEntity : class
    {
        #region Fields
        private EntityDescriptor _descriptor;
        private MemberDescriptor[] _keys;
        private MemberDescriptor[] _members;
        private MemberDescriptor[] _outputs;

        private readonly Func<MemberDescriptor, bool> _setFilter;
        #endregion

        public class Exclude<TExclude> : UpdateOperationBuilder<TEntity> where TExclude : class
        {
            public Exclude() : base(MemberFilters.Exclude<TExclude>()) { }
        }

        public class Include<TInclude> : UpdateOperationBuilder<TEntity> where TInclude : class
        {
            public Include() : base(MemberFilters.Include<TInclude>()) { }
        }

        #region Constructor

        public UpdateOperationBuilder() : this(MemberFilters.None()) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="setFilter">更新字段过滤 用于仅更新与排除更新</param>
        UpdateOperationBuilder(Func<MemberDescriptor, bool> setFilter)
        {
            if (setFilter == null) throw new ArgumentNullException(nameof(setFilter));
            _setFilter = setFilter;
        }

        void Initialize(EntityDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (descriptor.Type != typeof(TEntity)) throw new InvalidOperationException("实体描述符与当前类型不匹配.");

            if (descriptor == _descriptor) return;  // 避免重复初始化

            var keys = descriptor.Keys;
            if (keys == null || keys.Length == 0)
                throw new InvalidOperationException($"更新目标类型{descriptor.Type}必须具备主键");

            _members = descriptor.Members.Except(keys).Where(_ => !_.DbGenerated).Where(_setFilter).ToArray();
            if (_members.Length == 0)
                throw new InvalidOperationException("未找到需要更新的列.");
            _descriptor = descriptor;
            _keys = keys;
            _outputs = descriptor.Members.Where(_ => _.DbGenerated && !_.PrimaryKey).ToArray(); // 仅返回非主键的更新部分
        }
        #endregion

        #region Methods
        static string BuildSetParameter(int index) { return BuildParameter("p_set_" + index); }

        static string BuildConditionParameter(int index) { return BuildParameter("p_where_" + index); }

        static Action<IDatabase, DbCommand, TEntity> CreateParameterBinder(MemberDescriptor[] descriptors, Func<int, string> parameterNameBuilder)
        {
            var members = descriptors.Select((_, i) => new MemberTuple<TEntity>(
                _.Name,
                _.DbType,
                parameterNameBuilder(i),
                MemberAccessor<TEntity, object>.GetGetter(_.Member)
            )).ToArray();
            return (db, command, entity) =>
            {
                foreach (var member in members)
                {
                    db.AddInParameter(command, member.ParameterName, member.DbType, member.Getter(entity));
                }
            };
        }

        internal Action<IDatabase, DbCommand, TEntity> BuildCommandExecutor()
        {
            // 不带返回
            if (_outputs.Length == 0) return (db, command, entity) => db.ExecuteNonQuery(command);

            var assigns = _outputs.Select(_ => MemberAccessor.GetAssign<TEntity>(_.Member)).ToArray();

            return (db, command, entity) =>
            {
                var result = db.ExecuteEntity<TEntity>(command);
                foreach (var assign in assigns) assign(result, entity); // 回写
            };
        }


        #endregion

        #region IIOperationBuilder Methods

        Func<IDatabase, DbCommand> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildCommandProvider(EntityDescriptor descriptor)
        {
            Initialize(descriptor);

            var sqlBuilder = new StringBuilder("UPDATE ")
                .Append(string.IsNullOrEmpty(_descriptor.Schema) ? null : (Quote(_descriptor.Schema) + '.'))
                .AppendLine(Quote(_descriptor.Name))
                .Append(" SET ")
                .AppendJoin(", ", _members.Select((_, i) => Quote(_.Name) + " = " + BuildSetParameter(i)))
                .AppendLine();
            if (_outputs.Length > 0)
            {
                sqlBuilder.Append(" OUTPUT ")
                    .AppendJoin(", ", _outputs.Select(_ => "INSERTED." + Quote(_.Name)))
                    .AppendLine();
            }
            sqlBuilder.Append(" WHERE ")
                    .AppendJoin(", ", _keys.Select((_, i) => Quote(_.Name) + " = " + BuildConditionParameter(i)));

            var sql = sqlBuilder.ToString();
            return db => db.GetSqlStringCommand(sql);
        }

        Action<IDatabase, DbCommand, TEntity> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildInvoker(EntityDescriptor descriptor)
        {
            Initialize(descriptor);

            var memberBinder = CreateParameterBinder(_members, BuildSetParameter);
            var conditionBinder = CreateParameterBinder(_keys, BuildConditionParameter);
            var executer = BuildCommandExecutor();

            return (db, command, entity) =>
            {
                memberBinder(db, command, entity);
                conditionBinder(db, command, entity);
                executer(db, command, entity);
            };
        }
        #endregion
    }
}
