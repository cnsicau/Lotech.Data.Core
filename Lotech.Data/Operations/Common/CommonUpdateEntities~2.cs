using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Lotech.Data.Descriptors;
using Lotech.Data.Utils;

namespace Lotech.Data.Operations.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TSet"></typeparam>
    public abstract class CommonUpdateEntities<TEntity, TSet>
        : IOperationProvider<Action<IDatabase, TEntity, Expression<Func<TEntity, bool>>>>
            where TEntity : class where TSet : class
    {
        private readonly Func<string, string> quote;
        private readonly Func<string, string> buildParameter;
        private readonly Func<IDatabase, SqlExpressionVisitor<TEntity>> visitorProvider;

        /// <summary>
        /// 
        /// </summary>
        protected CommonUpdateEntities(Func<string, string> quote, Func<string, string> buildParameter, Func<IDatabase, SqlExpressionVisitor<TEntity>> visitorProvider)
        {
            this.quote = quote;
            this.buildParameter = buildParameter ?? (_ => _);
            this.visitorProvider = visitorProvider;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public Action<IDatabase, TEntity, Expression<Func<TEntity, bool>>> Create(EntityDescriptor descriptor)
        {
            var sets = AttributeDescriptorFactory.Create<TSet>().Members.Select(_ => _.Member.Name).ToArray();
            var members = descriptor.Members.Where(_ => sets.Contains(_.Member.Name)).Select((_, i) =>
                new MemberTuple<TEntity>(
                 _.Name,
                 _.DbType,
                 buildParameter("p_set_" + i),
                 MemberAccessor<TEntity, object>.GetGetter(_.Member)
             )).ToArray();
            if (members.Length == 0)
                throw new InvalidOperationException("未设置更新列集合");

            if (quote != null)
            {
                var sql = new StringBuilder("UPDATE ")
                   .Append(string.IsNullOrEmpty(descriptor.Schema) ? null : (quote(descriptor.Schema) + '.'))
                   .AppendLine(quote(descriptor.Name))
                   .Append(" SET ")
                   .AppendJoin(", ", members.Select(_ => quote(_.Name) + " = " + _.ParameterName))
                   .AppendLine()
                   .Append(" WHERE ").ToString();

                return (db, entity, predicate) =>
                {
                    if (entity == null) throw new ArgumentNullException(nameof(entity));
                    if (predicate == null) throw new ArgumentNullException(nameof(predicate));

                    var visitor = visitorProvider(db);
                    visitor.AddFragment(sql);
                    visitor.Visit(predicate);
                    using (var command = db.GetSqlStringCommand(visitor.Sql))
                    {
                        foreach (var member in members)
                        {
                            db.AddInParameter(command, member.ParameterName, member.DbType, member.Getter(entity));
                        }
                        foreach (var p in visitor.Parameters)
                        {
                            db.AddInParameter(command, p.Name, DbTypeParser.Parse(p.Type), p.Value);
                        }
                        db.ExecuteNonQuery(command);
                    }
                };
            }
            return (db, entity, predicate) =>
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));
                if (predicate == null) throw new ArgumentNullException(nameof(predicate));

                var sql = new StringBuilder("UPDATE ")
                   .Append(string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.'))
                   .AppendLine(db.QuoteName(descriptor.Name))
                   .Append(" SET ")
                   .AppendJoin(", ", members.Select(_ => db.QuoteName(_.Name) + " = " + db.BuildParameterName(_.ParameterName)))
                   .AppendLine()
                   .Append(" WHERE ").ToString();

                var visitor = visitorProvider(db);
                visitor.AddFragment(sql);
                visitor.Visit(predicate);
                using (var command = db.GetSqlStringCommand(visitor.Sql))
                {
                    foreach (var member in members)
                    {
                        db.AddInParameter(command, member.ParameterName, member.DbType, member.Getter(entity));
                    }
                    foreach (var p in visitor.Parameters)
                    {
                        db.AddInParameter(command, p.Name, DbTypeParser.Parse(p.Type), p.Value);
                    }
                    db.ExecuteNonQuery(command);
                }
            };
        }
    }
}
