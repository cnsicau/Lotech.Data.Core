using Lotech.Data.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lotech.Data.Operations.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class CommonFindEntities<TEntity> : IOperationProvider<Func<IDatabase, TEntity[]>>
        where TEntity : class
    {
        private readonly Func<string, string> quote;

        /// <summary>
        /// 
        /// </summary>
        protected CommonFindEntities() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quote"></param>
        public CommonFindEntities(Func<string, string> quote)
        {
            this.quote = quote;
        }

        Func<IDatabase, TEntity[]> IOperationProvider<Func<IDatabase, TEntity[]>>.Create(EntityDescriptor descriptor)
        {
            if (quote != null)
            {
                var sql = string.Concat("SELECT "
                                        , string.Join(", ", descriptor.Members.Select(_ => quote(_.Name)))
                                        , " FROM "
                                        , string.IsNullOrEmpty(descriptor.Schema) ? null : (quote(descriptor.Schema) + '.')
                                        , quote(descriptor.Name));

                return db => db.ExecuteEntities<TEntity>(sql);
            }

            return db =>
            {
                var sql = string.Concat("SELECT "
                                        , string.Join(", ", descriptor.Members.Select(_ => db.QuoteName(_.Name)))
                                        , " FROM "
                                        , string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.')
                                        , db.QuoteName(descriptor.Name));
                return db.ExecuteEntities<TEntity>(sql);
            };
        }
    }
}
