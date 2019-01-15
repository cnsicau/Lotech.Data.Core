using System;
using System.Collections.Generic;
using System.Linq;
using Lotech.Data.Descriptors;
using Lotech.Data.Utils;

namespace Lotech.Data.Oracles
{
    using System.Data.Common;
    using static OracleDatabase;

    class OracleDeleteEntities<TEntity> : IOperationProvider<Action<IDatabase, IEnumerable<TEntity>>>
        where TEntity : class
    {
        [ThreadStatic]
        internal static Action<DbCommand, int> ArrayBind;

        static string BuildParameterName(int index) { return BuildParameter("p" + index); }

        public Action<IDatabase, IEnumerable<TEntity>> Create(IEntityDescriptor descriptor)
        {
            if (descriptor.Keys == null || descriptor.Keys.Length == 0)
                throw new InvalidOperationException("仅支持具备主键数据表的删除操作.");

            var keys = descriptor.Keys.Select((key, i) => new MemberTuple<TEntity>
                   (
                       key.Name,
                       key.DbType,
                       BuildParameterName(i),
                       MemberAccessor<TEntity, object>.GetGetter(key.Member)
                   )).ToArray();

            var sql = string.Concat("DELETE FROM "
                                    , string.IsNullOrEmpty(descriptor.Schema) ? null : (Quote(descriptor.Schema) + '.')
                                    , Quote(descriptor.Name)
                                    , " WHERE "
                                    , string.Join(", ", keys.Select(_ => Quote(_.Name) + " = " + _.ParameterName)));

            return (db, entities) =>
            {
                var entitiyList = (entities as IList<TEntity>) ?? entities.ToArray();
                var parameters = new object[keys.Length][];

                #region Prepare ArrayBind Parameters

                for (int i = 0; i < keys.Length; i++)
                {
                    parameters[i] = new object[entitiyList.Count];
                    int index = 0;
                    var get = keys[i].Getter;
                    foreach (var entity in entitiyList)
                    {
                        parameters[i][index++] = get(entity);
                    }
                }
                #endregion

                using (var command = db.GetSqlStringCommand(sql))
                {
                    // bind input parameters
                    for (int i = 0; i < keys.Length; i++)
                    {
                        db.AddInParameter(command, BuildParameterName(i), keys[i].DbType, parameters[i]);
                    }

                    ArrayBind(command, entitiyList.Count);
                    db.ExecuteNonQuery(command);
                }
            };
        }
    }
}
