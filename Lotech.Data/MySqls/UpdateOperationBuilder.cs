using Lotech.Data.Descriptors;
using Lotech.Data.Operations;
using Lotech.Data.Utils;
using System;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Lotech.Data.MySqls
{
    using static MySqlDatabase;

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

            _outputs = descriptor.Members.Where(_ => _.DbGenerated && !_.PrimaryKey).ToArray();
        }
        #endregion

        #region Methods
        static string BuildSetParameter(int index) { return BuildParameter("p_set_" + index); }

        static string BuildConditionParameter(int index) { return BuildParameter("p_where_" + index); }

        static Action<IDatabase, DbCommand, TEntity> CreateParameterBinder(MemberDescriptor[] descriptors, Func<int, string> parameterNameBuilder)
        {
            var members = descriptors.Select((_, i) => new
            {
                _.Name,
                ParameterName = parameterNameBuilder(i),
                _.DbType,
                Value = MemberAccessor<TEntity, object>.GetGetter(_.Member)
            }).ToArray();
            return (db, command, entity) =>
            {
                foreach (var member in members)
                {
                    db.AddInParameter(command, member.ParameterName, member.DbType, member.Value(entity));
                }
            };
        }

        internal Action<IDatabase, DbCommand, TEntity> BuildCommandExecutor()
        {
            // 不带返回
            if (_outputs.Length == 0) return (db, command, entity) => db.ExecuteNonQuery(command);
            Action<IDatabase, DbCommand, TEntity> reverseBind = (db, command, entity) => { };

            if (_outputs.Length > 0)
            {
                var assigns = _outputs.Select(_ =>
                {
                    var get = MemberAccessor.GetGetter(_.Member);
                    var set = MemberAccessor.GetSetter(_.Member);

                    return new Action<TEntity, TEntity>(
                        (reverseEntity, entity) => set(entity, get(reverseEntity))
                    );
                });
                reverseBind = (db, command, entity) =>
                {
                    var reverseEntity = db.LoadEntity(entity);
                    foreach (var assign in assigns)
                        assign(reverseEntity, entity);
                };
            }

            return (db, command, entity) =>
            {
                using (var transactionManager = new TransactionManager())
                {
                    db.ExecuteNonQuery(command);
                    reverseBind(db, command, entity);
                }
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

            return (db, command, entity) =>
            {
                memberBinder(db, command, entity);
                conditionBinder(db, command, entity);
                db.ExecuteNonQuery(command);
            };
        }
        #endregion
    }
}
