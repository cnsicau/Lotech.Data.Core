using Lotech.Data.Descriptors;
using System;
using System.Linq;

namespace Lotech.Data.Operations.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class CommonLoadEntity<TEntity> : IOperationProvider<Func<IDatabase, TEntity, TEntity>>
        where TEntity : class
    {
        private IOperationProvider<Func<IDatabase, TEntity, TEntity>> provider;

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

        Func<IDatabase, TEntity, TEntity> IOperationProvider<Func<IDatabase, TEntity, TEntity>>.Create(EntityDescriptor descriptor)
        {
            return provider.Create(descriptor);
        }

        #region Classes
        class Optimized : IOperationProvider<Func<IDatabase, TEntity, TEntity>>
        {
            private readonly Func<string, string> quote;
            private readonly Func<string, string> buildParameter;

            internal Optimized(Func<string, string> quote, Func<string, string> buildParameter)
            {
                this.quote = quote;
                this.buildParameter = buildParameter;
            }

            Func<IDatabase, TEntity, TEntity> IOperationProvider<Func<IDatabase, TEntity, TEntity>>.Create(EntityDescriptor descriptor)
            {
                if (descriptor.Keys == null || descriptor.Keys.Length == 0)
                    return (db, _) => throw new InvalidOperationException("仅支持具备主键数据表的加载操作.");

                var keys = descriptor.Keys.Select((key, index) =>
                        new
                        {
                            Name = quote(key.Name),
                            key.DbType,
                            ParameterName = buildParameter("p_sql_" + index),
                            Get = Utils.MemberAccessor<TEntity, object>.GetGetter(key.Member)
                        }).ToArray();

                var sql = string.Concat("SELECT "
                                        , string.Join(", ", descriptor.Members.Select(_ => quote(_.Name)))
                                        , " FROM "
                                        , string.IsNullOrEmpty(descriptor.Schema) ? null : (quote(descriptor.Schema) + '.')
                                        , quote(descriptor.Name)
                                        , " WHERE "
                                        , string.Join(", ", keys.Select(_ => _.Name + " = " + _.ParameterName)));
                return (db, entity) =>
                {
                    using (var command = db.GetSqlStringCommand(sql))
                    {
                        foreach (var key in keys)
                        {
                            db.AddInParameter(command, key.ParameterName, key.DbType, key.Get(entity));
                        }
                        return db.ExecuteEntity<TEntity>(command);
                    }
                };
            }
        }

        class Normal : IOperationProvider<Func<IDatabase, TEntity, TEntity>>
        {
            Func<IDatabase, TEntity, TEntity> IOperationProvider<Func<IDatabase, TEntity, TEntity>>.Create(EntityDescriptor descriptor)
            {
                if (descriptor.Keys == null || descriptor.Keys.Length == 0)
                    throw new InvalidOperationException("仅支持具备主键数据表的加载操作.");

                var keys = descriptor.Keys.Select((key, index) =>
                        new
                        {
                            key.Name,
                            key.DbType,
                            ParameterName = "p_sql_" + index,
                            Get = Utils.MemberAccessor<TEntity, object>.GetGetter(key.Member)
                        }).ToArray();
                return (db, entity) =>
                {
                    var sql = string.Concat("SELECT "
                                            , string.Join(", ", descriptor.Members.Select(_ => db.QuoteName(_.Name)))
                                            , " FROM "
                                            , string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.')
                                            , db.QuoteName(descriptor.Name)
                                            , " WHERE "
                                            , string.Join(", ", keys.Select(_ => db.QuoteName(_.Name) + " = " + db.BuildParameterName(_.ParameterName))));

                    using (var command = db.GetSqlStringCommand(sql))
                    {
                        foreach (var key in keys)
                        {
                            db.AddInParameter(command, db.BuildParameterName(key.ParameterName), key.DbType, key.Get(entity));
                        }
                        return db.ExecuteEntity<TEntity>(command);
                    }
                };
            }
        }
        #endregion
    }
}
