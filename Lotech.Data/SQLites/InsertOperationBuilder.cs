using Lotech.Data.Descriptors;
using Lotech.Data.Operations;
using Lotech.Data.Utils;
using System;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Lotech.Data.SQLites
{
    using static SQLiteDatabase;

    /// <summary>
    /// 
    /// </summary>
    class InsertOperationBuilder<TEntity> : IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>
        where TEntity : class
    {
        #region Fields
        private IEntityDescriptor _descriptor;
        private IMemberDescriptor[] _keys;
        private IMemberDescriptor[] _members;
        private IMemberDescriptor _identity; //返回的主键列
        private IMemberDescriptor[] _outputs; // 除主键外的返回列

        private readonly Func<IMemberDescriptor, bool> _setFilter;
        #endregion

        #region Constructor
        public InsertOperationBuilder() : this(MemberFilters.None()) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setFilter">更新字段过滤 用于仅更新与排除更新</param>
        InsertOperationBuilder(Func<IMemberDescriptor, bool> setFilter)
        {
            if (setFilter == null) throw new ArgumentNullException(nameof(setFilter));
            _setFilter = setFilter;

        }

        void Initialize(IEntityDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (descriptor.Type != typeof(TEntity)) throw new InvalidOperationException("实体描述符与当前类型不匹配.");

            if (descriptor == _descriptor) return;  // 避免重复初始化

            _members = descriptor.Members.Where(_ => !_.DbGenerated && _setFilter(_)).ToArray();
            if (_members.Length == 0)
                throw new InvalidOperationException("未找到需要更新的列.");
            _descriptor = descriptor;
            _keys = descriptor.Keys;
            var outputs = descriptor.Members.Where(_ => _.DbGenerated).ToArray();
            _identity = outputs.SingleOrDefault(_ => _.PrimaryKey);
            _outputs = outputs.Where(_ => !_.PrimaryKey).ToArray();
        }
        #endregion

        #region Methods
        static string BuildParameterName(int index) { return BuildParameter("p_sql_" + index); }

        internal Action<IDatabase, DbCommand, TEntity> BuildCommandExecutor()
        {
            // 不带返回
            if (_identity == null && _outputs.Length == 0)
                return (db, command, entity) => db.ExecuteNonQuery(command);
            if (_identity != null && _outputs.Length == 0) // 仅返回自增长主键
            {
                var setter = MemberAccessor<TEntity, object>.GetSetter(_identity.Member);
                return (db, command, entity) => setter(entity, db.ExecuteScalar(command));
            }

            var reverseAssigns = _outputs.Concat(_identity == null ? new IMemberDescriptor[0] : new[] { _identity })
                    .Select(_ => MemberAccessor.GetAssign<TEntity>(_.Member)).ToArray();

            return (db, command, entity) =>
            {
                var reverse = db.ExecuteEntity<TEntity>(command);
                foreach (var assign in reverseAssigns)
                    assign(reverse, entity);
            };
        }
        #endregion

        #region IIOperationBuilder Methods

        Func<IDatabase, DbCommand> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildCommandProvider(IEntityDescriptor descriptor)
        {
            Initialize(descriptor);

            var sql = new StringBuilder("INSERT INTO ")
                .Append(string.IsNullOrEmpty(_descriptor.Schema) ? null : (Quote(_descriptor.Schema) + '.'))
                .Append(Quote(_descriptor.Name))
                .Append("(")
                .AppendJoin(", ", _members.Select((_, i) => Quote(_.Name)))
                .AppendLine(")")
                .Append(" VALUES (")
                .AppendJoin(", ", _members.Select((_, i) => BuildParameterName(i)))
                .Append(")")
                .ToString();

            if (_identity != null && _outputs.Length == 0) // 仅返回自增长主键
            {
                sql += ";\n SELECT LAST_INSERT_ROWID()";
            }
            else if (_outputs.Length > 0)
            {
                sql += string.Concat(";\n SELECT "
                        , _identity == null ? "" : (Quote(_identity.Name) + ", ")
                        , string.Join(", ", _outputs.Select(_ => Quote(_.Name)))
                        , " FROM "
                        , string.IsNullOrEmpty(_descriptor.Schema) ? null : (Quote(_descriptor.Schema) + '.')
                        , Quote(_descriptor.Name)
                        , " WHERE "
                        , string.Join(", ", _descriptor.Keys.Select((_, i) => _.Name + " = "
                                + (_ == _identity ? "LAST_INSERT_ROWID()" : BuildParameterName(i + _members.Length)))));
            }
            return db => db.GetSqlStringCommand(sql);
        }

        Action<IDatabase, DbCommand, TEntity> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildInvoker(IEntityDescriptor descriptor)
        {
            Initialize(descriptor);

            var members = _members.Select((_, i) => new MemberTuple<TEntity>
            (
                _.Name,
                _.DbType,
                BuildParameterName(i),
                MemberAccessor<TEntity, object>.GetGetter(_.Member)
            )).
            Concat(_keys.Select((_, i) => new MemberTuple<TEntity>
                (
                    _.Name,
                    _.DbType,
                    BuildParameterName(i),
                    MemberAccessor<TEntity, object>.GetGetter(_.Member)
                ))
                .Where(_ => _outputs.Length > 0)
                .Where(_ => _.Name != _identity?.Name)
            ).
            ToArray();

            var executer = BuildCommandExecutor();

            return (db, command, entity) =>
            {
                foreach (var member in members)
                {
                    db.AddInParameter(command, member.ParameterName, member.DbType, member.Getter(entity));
                }
                executer(db, command, entity);
            };
        }
        #endregion
    }
}
