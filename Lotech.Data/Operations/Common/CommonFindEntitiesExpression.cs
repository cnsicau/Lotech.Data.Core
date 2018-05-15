using Lotech.Data.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Lotech.Data.Operations.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class CommonFindEntitiesExpression<TEntity> : IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity[]>>
        where TEntity : class
    {
        private readonly Func<string, string> quote;
        private readonly Func<IDatabase, SqlExpressionVisitor<TEntity>> visitorProvider;

        /// <summary>
        /// 
        /// </summary>
        protected CommonFindEntitiesExpression() : this(db => new SqlExpressionVisitor<TEntity>(db)) { }

        /// <summary>
        /// 
        /// </summary>
        protected CommonFindEntitiesExpression(Func<IDatabase, SqlExpressionVisitor<TEntity>> visitorProvider)
        {
            this.visitorProvider = visitorProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visitorProvider"></param>
        /// <param name="quote"></param>
        public CommonFindEntitiesExpression(Func<IDatabase, SqlExpressionVisitor<TEntity>> visitorProvider, Func<string, string> quote) : this(visitorProvider)
        {
            this.quote = quote;
        }

        Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity[]>
            IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity[]>>.Create(EntityDescriptor descriptor)
        {
            if (quote != null)
            {
                var sql = string.Concat("SELECT "
                                        , string.Join(", ", descriptor.Members.Select(_ => quote(_.Name)))
                                        , " FROM "
                                        , string.IsNullOrEmpty(descriptor.Schema) ? null : (quote(descriptor.Schema) + '.')
                                        , quote(descriptor.Name)
                                        , " WHERE ");

                return (db, predicate) =>
                {
                    if (predicate == null) throw new ArgumentNullException(nameof(predicate));

                    var visitor = visitorProvider(db);
                    using (var command = visitor.CreateCommand(sql, predicate))
                    {
                        return db.ExecuteEntities<TEntity>(command);
                    }
                };
            }
            return (db, predicate) =>
            {
                if (predicate == null) throw new ArgumentNullException(nameof(predicate));

                var sql = string.Concat("SELECT "
                                        , string.Join(", ", descriptor.Members.Select(_ => db.QuoteName(_.Name)))
                                        , " FROM "
                                        , string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.')
                                        , db.QuoteName(descriptor.Name)
                                        , " WHERE ");
                using (var command = visitorProvider(db).CreateCommand(sql, predicate))
                {
                    return db.ExecuteEntities<TEntity>(command);
                }
            };
        }
    }
}
