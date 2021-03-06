﻿using Lotech.Data.Descriptors;
using Lotech.Data.Operations;
using Lotech.Data.Utils;
using System;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Lotech.Data.Oracles
{
    using static OracleDatabase;

    /// <summary>
    /// 
    /// </summary>
    class InsertOperationBuilder<TEntity> : IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>
        where TEntity : class
    {
        #region Fields
        private IEntityDescriptor _descriptor;
        private IMemberDescriptor[] _members;
        private IMemberDescriptor[] _outputs;

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
            _setFilter = setFilter ?? throw new ArgumentNullException(nameof(setFilter));

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
            _outputs = descriptor.Members.Where(_ => _.DbGenerated).ToArray();
        }
        #endregion

        #region Methods
        static string BuildParameterName(int index) { return BuildParameter("p_sql_" + index); }

        Action<IDatabase, DbCommand, TEntity> CreateParameterBinder()
        {
            var members = _members.Select((_, i) => new MemberTuple<TEntity>
            (
                _.Name,
                _.DbType,
                BuildParameterName(i),
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

            var outputParameters = _outputs.Select((_, index) =>
            {
                var parameterIndex = _members.Length + index;
                var assign = MemberAccessor<TEntity, object>.GetSetter(_.Member);
                return new
                {
                    Size = _.Type.IsValueType ? 64 : 4000,
                    _.DbType,
                    ParameterName = BuildParameterName(parameterIndex),
                    Assign = new Action<DbCommand, TEntity>((command, entity) => assign(entity, command.Parameters[parameterIndex].Value))
                };
            }).ToArray();

            return (db, command, entity) =>
            {
                foreach (var p in outputParameters)
                    db.AddOutParameter(command, p.ParameterName, p.DbType, p.Size);

                var result = db.ExecuteEntity<TEntity>(command);

                foreach (var p in outputParameters) p.Assign(command, entity);
            };
        }


        #endregion

        #region IIOperationBuilder Methods

        Func<IDatabase, DbCommand> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildCommandProvider(IEntityDescriptor descriptor)
        {
            Initialize(descriptor);

            var sqlBuilder = new StringBuilder("INSERT INTO ")
                .Append(string.IsNullOrEmpty(_descriptor.Schema) ? null : (Quote(_descriptor.Schema) + '.'))
                .Append(Quote(_descriptor.Name))
                .Append("(")
                .AppendJoin(", ", _members.Select((_, i) => Quote(_.Name)))
                .AppendLine(")")
                .Append(" VALUES (")
                .AppendJoin(", ", _members.Select((_, i) => BuildParameterName(i)))
                .AppendLine(")");

            if (_outputs.Length > 0)
            {
                sqlBuilder.Append(" RETURNING ")
                    .AppendJoin(", ", _outputs.Select(_ => Quote(_.Name)))
                    .Append(" INTO ")
                    .AppendJoin(", ", _outputs.Select((_, i) => BuildParameterName(i + _members.Length)));
            }

            var sql = sqlBuilder.ToString();
            return db => db.GetSqlStringCommand(sql);
        }

        Action<IDatabase, DbCommand, TEntity> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildInvoker(IEntityDescriptor descriptor)
        {
            Initialize(descriptor);

            var memberBinder = CreateParameterBinder();
            var executer = BuildCommandExecutor();

            return (db, command, entity) =>
            {
                memberBinder(db, command, entity);
                executer(db, command, entity);
            };
        }
        #endregion
    }
}
