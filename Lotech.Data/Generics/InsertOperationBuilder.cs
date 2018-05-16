using Lotech.Data.Descriptors;
using Lotech.Data.Operations;
using Lotech.Data.Utils;
using System;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Lotech.Data.Generics
{
    class InsertOperationBuilder<TEntity> : IOperationBuilder<Action<IDatabase, DbCommand, TEntity>> where TEntity : class
    {
        MemberTuple<TEntity>[] members;
        EntityDescriptor descriptor;

        void Initialize(EntityDescriptor descriptor)
        {
            if (this.descriptor == descriptor) return;
            this.descriptor = descriptor;
            members = descriptor.Members.Where(_ => !_.DbGenerated) // 忽略库中生成的属性
                .Select((member, index) => new MemberTuple<TEntity>(member.Name,
                          member.DbType,
                           "p_sql_" + index,
                          Utils.MemberAccessor<TEntity, object>.GetGetter(member.Member)
                        )).ToArray();

        }

        Func<IDatabase, DbCommand> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildCommandProvider(EntityDescriptor descriptor)
        {
            Initialize(descriptor);

            return db =>
            {
                var sql = new StringBuilder("INSERT INTO ")
                    .Append(string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.'))
                    .Append(db.QuoteName(descriptor.Name))
                    .Append("(")
                    .AppendJoin(", ", members.Select(_ => db.QuoteName(_.Name)))
                    .Append(") VALUES (")
                    .AppendJoin(", ", members.Select(_ => db.BuildParameterName(_.ParameterName)))
                    .Append(')')
                    .ToString();

                return db.GetSqlStringCommand(sql);
            };
        }

        Action<IDatabase, DbCommand, TEntity> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildInvoker(EntityDescriptor descriptor)
        {
            Initialize(descriptor);
            return (db, command, entity) =>
            {
                foreach (var member in members)
                {
                    db.AddInParameter(command, db.BuildParameterName(member.ParameterName), member.DbType, member.Getter(entity));
                }
                db.ExecuteNonQuery(command);
            };
        }
    }
}
