﻿using Lotech.Data.Descriptors;
using System;
using System.Linq.Expressions;

namespace Lotech.Data.Operations.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class CommonCountEntitiesExpression<TEntity> : IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, int>>
        where TEntity : class
    {
        private readonly Func<string, string> quote;
        private readonly Func<IDatabase, SqlExpressionVisitor<TEntity>> visitorProvider;

        /// <summary>
        /// 
        /// </summary>
        protected CommonCountEntitiesExpression() : this(db => new SqlExpressionVisitor<TEntity>(db, Operation.Select)) { }

        /// <summary>
        /// 
        /// </summary>
        protected CommonCountEntitiesExpression(Func<IDatabase, SqlExpressionVisitor<TEntity>> visitorProvider)
        {
            this.visitorProvider = visitorProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visitorProvider"></param>
        /// <param name="quote"></param>
        public CommonCountEntitiesExpression(Func<IDatabase, SqlExpressionVisitor<TEntity>> visitorProvider, Func<string, string> quote) : this(visitorProvider)
        {
            this.quote = quote;
        }

        Func<IDatabase, Expression<Func<TEntity, bool>>, int>
            IOperationProvider<Func<IDatabase, Expression<Func<TEntity, bool>>, int>>.Create(IEntityDescriptor descriptor)
        {
            if (quote != null)
            {
                var sql = string.Concat("SELECT COUNT(1) FROM "
                                        , string.IsNullOrEmpty(descriptor.Schema) ? null : (quote(descriptor.Schema) + '.')
                                        , quote(descriptor.Name)
                                        , " WHERE ");

                return (db, predicate) =>
                {
                    if (predicate == null) throw new ArgumentNullException(nameof(predicate));

                    var visitor = visitorProvider(db);
                    using (var command = visitor.CreateCommand(sql, predicate))
                    {
                        return db.ExecuteScalar<int>(command);
                    }
                };
            }
            return (db, predicate) =>
            {
                if (predicate == null) throw new ArgumentNullException(nameof(predicate));

                var sql = string.Concat("SELECT COUNT(1) FROM "
                                        , string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.')
                                        , db.QuoteName(descriptor.Name)
                                        , " WHERE ");
                using (var command = visitorProvider(db).CreateCommand(sql, predicate))
                {
                    return db.ExecuteScalar<int>(command);
                }
            };
        }
    }
}
