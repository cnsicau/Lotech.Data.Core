using System;
using System.Data.Common;
using System.Linq;
using Lotech.Data.Descriptors;
using Lotech.Data.Operations;

namespace Lotech.Data.Generics
{
    class UpdateOperationBuilder<TEntity> : IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>
        where TEntity : class
    {
        public class Exclude<TExclude> : UpdateOperationBuilder<TEntity> where TExclude : class
        {
            public Exclude() : base(MemberFilters.Exclude<TExclude>()) { }
        }

        public class Include<TInclude> : UpdateOperationBuilder<TEntity> where TInclude : class
        {
            public Include() : base(MemberFilters.Include<TInclude>()) { }
        }

        public UpdateOperationBuilder() : this(MemberFilters.None()) { }

        private readonly Func<MemberDescriptor, bool> _setFilter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setFilter">更新字段过滤 用于仅更新与排除更新</param>
        UpdateOperationBuilder(Func<MemberDescriptor, bool> setFilter)
        {
            _setFilter = setFilter ?? throw new ArgumentNullException(nameof(setFilter));
        }

        Func<IDatabase, DbCommand> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildCommandProvider(EntityDescriptor descriptor)
        {
            if (descriptor.Keys == null || descriptor.Keys.Length == 0)
                throw new InvalidOperationException("仅支持具备主键数据表的更新操作.");

            // 过滤非主键、非库生成的成员
            var members = descriptor.Members.Except(descriptor.Keys)
                .Where(_ => !_.DbGenerated && _setFilter(_))
                .Select((key, index) =>
                    new
                    {
                        key.Name,
                        ParameterName = "p_set_" + index,
                    }).ToArray();

            if (members.Length == 0)
                throw new InvalidOperationException("未找到需要更新的列.");

            var keys = descriptor.Keys.Select((key, index) =>
                    new
                    {
                        key.Name,
                        ParameterName = "p_where_" + index,
                    }).ToArray();

            return db =>
            {
                var sql = string.Concat("UPDATE "
                                           , string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.')
                                           , db.QuoteName(descriptor.Name)
                                           , " SET "
                                           , string.Join(", ", members.Select(_ => db.QuoteName(_.Name) + " = " + db.BuildParameterName(_.ParameterName)))
                                           , " WHERE "
                                           , string.Join(", ", keys.Select(_ => db.QuoteName(_.Name) + " = " + db.BuildParameterName(_.ParameterName))));
                return db.GetSqlStringCommand(sql);
            };
        }

        Action<IDatabase, DbCommand, TEntity> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildInvoker(EntityDescriptor descriptor)
        {
            if (descriptor.Keys == null || descriptor.Keys.Length == 0)
                throw new InvalidOperationException("仅支持具备主键数据表的更新操作.");

            // 过滤非主键、非库生成的成员
            var members = descriptor.Members.Except(descriptor.Keys)
                .Where(_ => !_.DbGenerated)
                .Where(_setFilter)
                .Select((key, index) =>
                    new
                    {
                        key.DbType,
                        ParameterName = "p_set_" + index,
                        Get = Utils.MemberAccessor<TEntity, object>.GetGetter(key.Member)
                    }).ToArray();

            if (members.Length == 0)
                throw new InvalidOperationException("未找到需要更新的列.");

            var keys = descriptor.Keys.Select((key, index) =>
                    new
                    {
                        key.DbType,
                        ParameterName = "p_where_" + index,
                        Get = Utils.MemberAccessor<TEntity, object>.GetGetter(key.Member)
                    }).ToArray();

            return (db, command, entity) =>
            {
                foreach (var member in members)
                {
                    db.AddInParameter(command, db.BuildParameterName(member.ParameterName), member.DbType, member.Get(entity));
                }
                foreach (var key in keys)
                {
                    db.AddInParameter(command, db.BuildParameterName(key.ParameterName), key.DbType, key.Get(entity));
                }
                db.ExecuteNonQuery(command);
            };
        }
    }
}
