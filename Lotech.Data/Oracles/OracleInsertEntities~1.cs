using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lotech.Data.Descriptors;
using Lotech.Data.Utils;

namespace Lotech.Data.Oracles
{
    using System.Data.Common;
    using static OracleDatabase;

    class OracleInsertEntities<TEntity> : IOperationProvider<Action<IDatabase, IEnumerable<TEntity>>>
        where TEntity : class
    {
        [ThreadStatic]
        internal static Action<DbCommand, int> ArrayBind;

        private IEntityDescriptor _descriptor;
        private IMemberDescriptor[] _keys;
        private IMemberDescriptor[] _members;
        private IMemberDescriptor[] _outputs;

        static string BuildParameterName(int index) { return BuildParameter("p" + index); }

        void Initialize(IEntityDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (descriptor.Type != typeof(TEntity)) throw new InvalidOperationException("实体描述符与当前类型不匹配.");

            if (descriptor == _descriptor) return;  // 避免重复初始化

            _members = descriptor.Members.Where(_ => !_.DbGenerated).ToArray();
            if (_members.Length == 0)
                throw new InvalidOperationException("insert members not found.");
            _descriptor = descriptor;
            _keys = descriptor.Keys;
            _outputs = descriptor.Members.Where(_ => _.DbGenerated).ToArray();
        }

        string GenerateInsertSql()
        {
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

            return sqlBuilder.ToString();
        }

        public Action<IDatabase, IEnumerable<TEntity>> Create(IEntityDescriptor descriptor)
        {
            Initialize(descriptor);
            var sql = GenerateInsertSql();
            var members = _members.Select((_, i) => new MemberTuple<TEntity>
                   (
                       _.Name,
                       _.DbType,
                       BuildParameterName(i),
                       MemberAccessor<TEntity, object>.GetGetter(_.Member)
                   )).ToArray();
            var outputs = _outputs.Select((_, index) =>
            {
                var parameterIndex = _members.Length + index;
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
