using Lotech.Data.Descriptors;
using Lotech.Data.Utils;
using System;
using System.Linq;

namespace Lotech.Data.Operations.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class CommonExistsEntity<TEntity, TKey> : IOperationProvider<Func<IDatabase, TKey, bool>>
        where TEntity : class
    {
        private readonly Func<string, string> quote;
        private readonly Func<string, string> buildParameter;

        /// <summary>
        /// 
        /// </summary>
        protected CommonExistsEntity() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="buildParameter"></param>

        internal CommonExistsEntity(Func<string, string> quote, Func<string, string> buildParameter)
        {
            this.quote = quote;
            this.buildParameter = buildParameter;
        }
        Func<IDatabase, TKey, bool> IOperationProvider<Func<IDatabase, TKey, bool>>.Create(EntityDescriptor descriptor)
        {
            if (descriptor.Keys?.Length != 1)
                throw new InvalidOperationException("仅支持单主键数据表的加载操作.");
            var key = descriptor.Keys.Single();
            var convert = ValueConverter.GetConvert(key.Type, typeof(TKey));

            if (quote != null)
            {

                var parameterName = buildParameter("p_sql_0");
                var sql = string.Concat("SELECT 1 FROM "
                                        , string.IsNullOrEmpty(descriptor.Schema) ? null : (quote(descriptor.Schema) + '.')
                                        , quote(descriptor.Name)
                                        , " WHERE "
                                        , quote(key.Name)
                                        , " = "
                                        , parameterName);
                return (db, id) =>
                {
                    using (var command = db.GetSqlStringCommand(sql))
                    {
                        db.AddInParameter(command, parameterName, key.DbType, convert(id));
                        return db.ExecuteScalar<int>(command) > 0;
                    }
                };
            }
            return (db, id) =>
                {
                    var keyParameter = db.BuildParameterName("p_sql_0");
                    var sql = string.Concat("SELECT 1 FROM "
                                            , string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.')
                                            , db.QuoteName(descriptor.Name)
                                            , " WHERE "
                                            , db.QuoteName(key.Name)
                                            , " = "
                                            , keyParameter);
                    using (var command = db.GetSqlStringCommand(sql))
                    {
                        db.AddInParameter(command, keyParameter, key.DbType, convert(id));
                        return db.ExecuteScalar<int>(command) > 0;
                    }
                };
        }
    }
}
