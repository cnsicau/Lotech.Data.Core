using Lotech.Data.Descriptors;
using Lotech.Data.Utils;
using System;
using System.Data.Common;

namespace Lotech.Data.Operations.Common
{
    /// <summary>
    /// 据主键删除
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class CommonDeleteEntity<TEntity, TKey> : IOperationProvider<Action<IDatabase, TKey>>
      where TEntity : class
    {
        class Normal : IOperationProvider<Action<IDatabase, TKey>>
        {
            Action<IDatabase, TKey> IOperationProvider<Action<IDatabase, TKey>>.Create(EntityDescriptor descriptor)
            {
                const string ParameterName = "p_sql_0";
                if (descriptor.Keys?.Length != 1)
                    throw new InvalidOperationException("仅支持单主键数据表的删除操作.");

                var key = descriptor.Keys[0];
                var convert = ValueConverter.GetConvert(key.Type, typeof(TKey));

                return (db, id) =>
                {
                    var sql = string.Concat("DELETE FROM "
                                                 , string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.')
                                                 , db.QuoteName(descriptor.Name)
                                                 , " WHERE "
                                                 , db.QuoteName(key.Name)
                                                 , " = "
                                                 , db.BuildParameterName(ParameterName));
                    using (var command = db.GetSqlStringCommand(sql))
                    {
                        db.AddInParameter(command, db.BuildParameterName(ParameterName), key.DbType, convert(id));
                        db.ExecuteNonQuery(command);
                    }
                };
            }
        }

        class Optmized : OperationProvider<TKey>, IOperationProvider<Action<IDatabase, TKey>>
        {
            class OptmizedBuilder : IOperationBuilder<Action<IDatabase, DbCommand, TKey>>
            {
                private readonly string parameterName;
                private readonly Func<string, string> quote;

                internal OptmizedBuilder(Func<string, string> quote, Func<string, string> buildParameter)
                {
                    parameterName = buildParameter("p_sql_0");
                    this.quote = quote;
                }
                public Func<IDatabase, DbCommand> BuildCommandProvider(EntityDescriptor descriptor)
                {
                    if (descriptor.Keys?.Length != 1)
                        throw new InvalidOperationException("仅支持单主键数据表的删除操作.");

                    var sql = string.Concat("DELETE FROM "
                                            , string.IsNullOrEmpty(descriptor.Schema) ? null : (quote(descriptor.Schema) + '.')
                                            , quote(descriptor.Name)
                                            , " WHERE "
                                            , quote(descriptor.Keys[0].Name)
                                            , " = "
                                            , parameterName);
                    return db => db.GetSqlStringCommand(sql);
                }

                public Action<IDatabase, DbCommand, TKey> BuildInvoker(EntityDescriptor descriptor)
                {
                    var dbType = descriptor.Keys[0].DbType;
                    var type = descriptor.Keys[0].Type;
                    var convert = ValueConverter.GetConvert(typeof(TKey), type);

                    return (db, command, id) =>
                    {
                        db.AddInParameter(command, parameterName, dbType, convert(id));
                        db.ExecuteNonQuery(command);
                    };
                }
            }

            internal Optmized(Func<string, string> quote, Func<string, string> buildParameter) : base(new OptmizedBuilder(quote, buildParameter)) { }
        }

        IOperationProvider<Action<IDatabase, TKey>> provider;
        /// <summary>
        /// 
        /// </summary>
        protected CommonDeleteEntity() { provider = new Normal(); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="buildParameter"></param>
        protected CommonDeleteEntity(Func<string, string> quote, Func<string, string> buildParameter)
        {
            provider = new Optmized(quote, buildParameter);
        }

        Action<IDatabase, TKey> IOperationProvider<Action<IDatabase, TKey>>.Create(EntityDescriptor descriptor)
        {
            return provider.Create(descriptor);
        }
    }
}
