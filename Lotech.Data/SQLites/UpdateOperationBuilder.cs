﻿using Lotech.Data.Descriptors;
using Lotech.Data.Operations;
using Lotech.Data.Utils;
using System;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Lotech.Data.SQLites
{
    using static SQLiteEntityServices;

    /// <summary>
    /// 
    /// </summary>
    class UpdateOperationBuilder<TEntity> : IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>
        where TEntity : class
    {
        #region Fields
        private IEntityDescriptor _descriptor;
        private IMemberDescriptor[] _keys;
        private IMemberDescriptor[] _members;
        private IMemberDescriptor[] _outputs;

        private readonly Func<IMemberDescriptor, bool> _setFilter;
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
        UpdateOperationBuilder(Func<IMemberDescriptor, bool> setFilter)
        {
            _setFilter = setFilter ?? throw new ArgumentNullException(nameof(setFilter));
        }

        void Initialize(IEntityDescriptor descriptor)
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

        internal Action<IDatabase, DbCommand, TEntity> BuildCommandExecutor()
        {
            // 不带返回
            if (_outputs.Length == 0) return (db, command, entity) => db.ExecuteNonQuery(command);


            var assigns = _outputs.Select(_ => MemberAccessor.GetAssign<TEntity>(_.Member)).ToArray();
            return (db, command, entity) =>
            {
                db.ExecuteNonQuery(command);
                var reverseEntity = db.LoadEntity(entity);
                foreach (var assign in assigns)
                    assign(reverseEntity, entity);
            };
        }
        #endregion

        #region IIOperationBuilder Methods

        Func<IDatabase, DbCommand> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildCommandProvider(IEntityDescriptor descriptor)
        {
            Initialize(descriptor);

            var sql = new StringBuilder("UPDATE ")
                .Append(string.IsNullOrEmpty(_descriptor.Schema) ? null : (Quote(_descriptor.Schema) + '.'))
                .AppendLine(Quote(_descriptor.Name))
                .Append(" SET ")
                .AppendJoin(", ", _members.Select((_, i) => Quote(_.Name) + " = " + BuildSetParameter(i)))
                .AppendLine()
                .Append(" WHERE ")
                .AppendJoin(" AND ", _keys.Select((_, i) => Quote(_.Name) + " = " + BuildConditionParameter(i)))
                .ToString();
            return db => db.GetSqlStringCommand(sql);
        }

        Action<IDatabase, DbCommand, TEntity> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildInvoker(IEntityDescriptor descriptor)
        {
            Initialize(descriptor);

            var parameters
                = _members.Select((_, i) => new MemberTuple<TEntity>(
                    _.Name,
                    _.DbType,
                    BuildSetParameter(i),
                    MemberAccessor<TEntity, object>.GetGetter(_.Member)
                ))
                .Concat(
                    _keys.Select((_, i) => new MemberTuple<TEntity>(
                        _.Name,
                        _.DbType,
                        BuildConditionParameter(i),
                        MemberAccessor<TEntity, object>.GetGetter(_.Member)
                ))).ToArray();

            return (db, command, entity) =>
            {
                foreach (var p in parameters)
                    db.AddInParameter(command, p.ParameterName, p.DbType, p.Getter(entity));

                db.ExecuteNonQuery(command);
            };
        }
        #endregion
    }
}
