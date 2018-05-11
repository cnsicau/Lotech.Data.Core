using Lotech.Data.Descriptors;
using System;
using System.Linq.Expressions;

namespace Lotech.Data.Operations.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class CommonDeleteEntitiesExpression<TEntity> : IOperationProvider<Action<IDatabase, Expression<Func<TEntity, bool>>>>
        where TEntity : class
    {
        private readonly Func<string, string> quote;
        private readonly Func<IDatabase, SqlExpressionVisitor<TEntity>> visitorProvider;

        /// <summary>
        /// 
        /// </summary>
        protected CommonDeleteEntitiesExpression() : this(db => new SqlExpressionVisitor<TEntity>(db)) { }

        /// <summary>
        /// 
        /// </summary>
        protected CommonDeleteEntitiesExpression(Func<IDatabase, SqlExpressionVisitor<TEntity>> visitorProvider)
        {
            this.visitorProvider = visitorProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visitorProvider"></param>
        /// <param name="quote"></param>
        public CommonDeleteEntitiesExpression(Func<IDatabase, SqlExpressionVisitor<TEntity>> visitorProvider, Func<string, string> quote) : this(visitorProvider)
        {
            this.quote = quote;
        }

        Action<IDatabase, Expression<Func<TEntity, bool>>>
            IOperationProvider<Action<IDatabase, Expression<Func<TEntity, bool>>>>.Create(EntityDescriptor descriptor)
        {
            if (quote != null)
            {
                var sql = string.Concat("DELETE FROM "
                                        , string.IsNullOrEmpty(descriptor.Schema) ? null : (quote(descriptor.Schema) + '.')
                                        , quote(descriptor.Name)
                                        , " WHERE ");

                return (db, predicate) =>
                {
                    if (predicate == null) throw new ArgumentNullException(nameof(predicate));

                    var visitor = visitorProvider(db);
                    using (var command = visitor.CreateCommand(sql, predicate))
                    {
                        db.ExecuteNonQuery(command);
                    }
                };
            }
            return (db, predicate) =>
            {
                if (predicate == null) throw new ArgumentNullException(nameof(predicate));

                var sql = string.Concat("DELETE FROM "
                                        , string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.')
                                        , db.QuoteName(descriptor.Name)
                                        , " WHERE ");
                using (var command = visitorProvider(db).CreateCommand(sql, predicate))
                {
                    db.ExecuteNonQuery(command);
                }
            };
        }
    }
}
