using Lotech.Data.Descriptors;
using System;

namespace Lotech.Data.Operations.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class CommonCountEntities<TEntity> : IOperationProvider<Func<IDatabase, int>>
        where TEntity : class
    {
        private readonly Func<string, string> quote;

        /// <summary>
        /// 
        /// </summary>
        protected CommonCountEntities() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quote"></param>
        public CommonCountEntities(Func<string, string> quote)
        {
            this.quote = quote;
        }

        Func<IDatabase, int> IOperationProvider<Func<IDatabase, int>>.Create(EntityDescriptor descriptor)
        {
            if (quote != null)
            {
                var sql = string.Concat("SELECT COUNT(1) FROM "
                                        , string.IsNullOrEmpty(descriptor.Schema) ? null : (quote(descriptor.Schema) + '.')
                                        , quote(descriptor.Name));

                return db => db.ExecuteScalar<int>(sql);
            }

            return db =>
            {
                var sql = string.Concat("SELECT COUNT(1) FROM "
                                        , string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.')
                                        , db.QuoteName(descriptor.Name));
                return db.ExecuteScalar<int>(sql);
            };
        }
    }
}
