using System;
using System.Linq;
using Lotech.Data.Descriptors;
using Lotech.Data.Utils;

namespace Lotech.Data.Operations.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class CommonLoadEntity<TEntity, TKey> : IOperationProvider<Func<IDatabase, TKey, TEntity>>
        where TEntity : class
    {
        private IOperationProvider<Func<IDatabase, TKey, TEntity>> provider;

        /// <summary>
        /// 
        /// </summary>
        protected CommonLoadEntity() { provider = new Normal(); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="buildParameter"></param>
        public CommonLoadEntity(Func<string, string> quote, Func<string, string> buildParameter)
        {
            provider = new Optimized(quote, buildParameter);
        }

        Func<IDatabase, TKey, TEntity> IOperationProvider<Func<IDatabase, TKey, TEntity>>.Create(EntityDescriptor descriptor)
        {
            return provider.Create(descriptor);
        }

        #region Classes
        class Optimized : IOperationProvider<Func<IDatabase, TKey, TEntity>>
        {
            private readonly Func<string, string> quote;
            private readonly Func<string, string> buildParameter;

            internal Optimized(Func<string, string> quote, Func<string, string> buildParameter)
            {
                this.quote = quote;
                this.buildParameter = buildParameter;
            }

            Func<IDatabase, TKey, TEntity> IOperationProvider<Func<IDatabase, TKey, TEntity>>.Create(EntityDescriptor descriptor)
            {
                if (descriptor.Keys?.Length != 1)
                    throw new InvalidOperationException("仅支持单主键数据表的加载操作.");
                var key = descriptor.Keys[0];
                var convert = ValueConverter.GetConvert(key.Type, typeof(TKey));

                var parameterName = buildParameter("p_sql_0");
                var sql = string.Concat("SELECT "
                                        , string.Join(", ", descriptor.Members.Select(_ => quote(_.Name)))
                                        , " FROM "
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
                        return db.ExecuteEntity<TEntity>(command);
                    }
                };
            }
        }

        class Normal : IOperationProvider<Func<IDatabase, TKey, TEntity>>
        {
            Func<IDatabase, TKey, TEntity> IOperationProvider<Func<IDatabase, TKey, TEntity>>.Create(EntityDescriptor descriptor)
            {
                if (descriptor.Keys?.Length != 1)
                    throw new InvalidOperationException("仅支持单主键数据表的加载操作.");
                var keyDescriptor = descriptor.Keys[0];
                var convert = ValueConverter.GetConvert(keyDescriptor.Type, typeof(TKey));

                return (db, key) =>
                {
                    var keyParameter = db.BuildParameterName("p_sql_0");
                    var sql = string.Concat("SELECT "
                                            , string.Join(", ", descriptor.Members.Select(_ => db.QuoteName(_.Name)))
                                            , " FROM "
                                            , string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.')
                                            , db.QuoteName(descriptor.Name)
                                            , " WHERE "
                                            , db.QuoteName(keyDescriptor.Name)
                                            , " = "
                                            , keyParameter);
                    using (var command = db.GetSqlStringCommand(sql))
                    {
                        db.AddInParameter(command, keyParameter, keyDescriptor.DbType, convert(key));
                        return db.ExecuteEntity<TEntity>(command);
                    }
                };
            }
        }
        #endregion
    }
}
