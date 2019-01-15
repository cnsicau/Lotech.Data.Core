using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lotech.Data.Descriptors;
using Lotech.Data.Utils;

namespace Lotech.Data.Oracles
{
    using System.Data.Common;
    using Operations;
    using static OracleDatabase;

    class OracleUpdateEntities<TEntity> : IOperationProvider<Action<IDatabase, IEnumerable<TEntity>>>
        where TEntity : class
    {
        [ThreadStatic]
        internal static Action<DbCommand, int> ArrayBind;

        private IEntityDescriptor _descriptor;
        private IMemberDescriptor[] _keys;
        private IMemberDescriptor[] _members;
        private IMemberDescriptor[] _outputs;
        private readonly Func<IMemberDescriptor, bool> _filter;

        public OracleUpdateEntities() : this(MemberFilters.None()) { }

        public OracleUpdateEntities(Func<IMemberDescriptor, bool> filter)
        {
            _filter = filter;
        }

        internal class Exclude<TExclude> : OracleUpdateEntities<TEntity> where TExclude : class
        {
            public Exclude() : base(MemberFilters.Exclude<TExclude>()) { }
        }
        internal class Include<TInclude> : OracleUpdateEntities<TEntity> where TInclude : class
        {
            public Include() : base(MemberFilters.Include<TInclude>()) { }
        }

        static string BuildParameterName(int index) { return BuildParameter("p" + index); }

        void Initialize(IEntityDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (descriptor.Type != typeof(TEntity)) throw new InvalidOperationException("实体描述符与当前类型不匹配.");

            if (descriptor == _descriptor) return;  // 避免重复初始化

            var keys = descriptor.Keys;
            if (keys == null || keys.Length == 0)
                throw new InvalidOperationException($"更新目标类型{descriptor.Type}必须具备主键");

            _members = descriptor.Members.Except(keys).Where(_ => !_.DbGenerated).Where(_filter).ToArray();
            if (_members.Length == 0)
                throw new InvalidOperationException("未找到需要更新的列.");
            _descriptor = descriptor;
            _keys = keys;
            _outputs = descriptor.Members.Where(_ => _.DbGenerated && !_.PrimaryKey).ToArray(); // 仅返回非主键的更新部分
        }

        string GenerateUpdateSql()
        {
            var sqlBuilder = new StringBuilder("UPDATE ")
                .Append(string.IsNullOrEmpty(_descriptor.Schema) ? null : (Quote(_descriptor.Schema) + '.'))
                .AppendLine(Quote(_descriptor.Name))
                .Append(" SET ")
                .AppendJoin(", ", _members.Select((_, i) => Quote(_.Name) + " = " + BuildParameterName(i)))
                .AppendLine();
            sqlBuilder.Append(" WHERE ")
                    .AppendJoin(", ", _keys.Select((_, i) => Quote(_.Name) + " = " + BuildParameterName(_members.Length + i)));

            if (_outputs.Length > 0)
            {
                sqlBuilder.Append(" RETURNING ")
                    .AppendJoin(", ", _outputs.Select(_ => Quote(_.Name)))
                    .Append(" INTO ")
                    .AppendJoin(", ", _outputs.Select((_, i) => BuildParameterName(i + _keys.Length + _members.Length)));
            }

            return sqlBuilder.ToString();
        }

        public Action<IDatabase, IEnumerable<TEntity>> Create(IEntityDescriptor descriptor)
        {
            Initialize(descriptor);
            var sql = GenerateUpdateSql();
            var members = _members.Concat(_keys).Select((_, i) => new MemberTuple<TEntity>
                   (
                       _.Name,
                       _.DbType,
                       BuildParameterName(i),
                       MemberAccessor<TEntity, object>.GetGetter(_.Member)
                   )).ToArray();
            var outputs = _outputs.Select((_, index) =>
            {
                var parameterIndex = members.Length + index;
                return new
                {
                    Size = _.Type.IsValueType ? 64 : 4000,
                    _.DbType,
                    Index = parameterIndex,
                    ParameterName = BuildParameterName(parameterIndex),
                    Setter = MemberAccessor<TEntity, object>.GetSetter(_.Member)
                };
            }).ToArray();

            return (db, entities) =>
            {
                var entitiyList = (entities as IList<TEntity>) ?? entities.ToArray();
                var parameters = new object[members.Length][];

                #region Prepare ArrayBind Parameters

                for (int i = 0; i < members.Length; i++)
                {
                    parameters[i] = new object[entitiyList.Count];
                    int index = 0;
                    var get = members[i].Getter;
                    foreach (var entity in entitiyList)
                    {
                        parameters[i][index++] = get(entity);
                    }
                }
                #endregion

                using (var command = db.GetSqlStringCommand(sql))
                {
                    // bind input parameters
                    for (int i = 0; i < members.Length; i++)
                    {
                        db.AddInParameter(command, BuildParameterName(i), members[i].DbType, parameters[i]);
                    }
                    //bind output parameters
                    foreach (var output in outputs)
                    {
                        db.AddOutParameter(command, BuildParameterName(output.Index), output.DbType, output.Size);
                    }

                    ArrayBind(command, entitiyList.Count);
                    db.ExecuteNonQuery(command);

                    #region Reverse Bind Output Parameters
                    foreach (var output in outputs)
                    {
                        var values = command.Parameters[output.Index].Value as System.Collections.IList;
                        for (int i = 0; i < entitiyList.Count; i++)
                        {
                            output.Setter(entitiyList[i], values[i]);
                        }
                    }
                    #endregion
                }
            };
        }
    }
}
