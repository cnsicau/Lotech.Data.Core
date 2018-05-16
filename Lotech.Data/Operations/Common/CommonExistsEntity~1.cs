using Lotech.Data.Descriptors;
using System;
using System.Data;
using System.Linq;

namespace Lotech.Data.Operations.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class CommonExistsEntity<TEntity> : IOperationProvider<Func<IDatabase, TEntity, bool>>
        where TEntity : class
    {
        private IOperationProvider<Func<IDatabase, TEntity, bool>> provider;

        /// <summary>
        /// 
        /// </summary>
        protected CommonExistsEntity() { provider = new Normal(); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="buildParameter"></param>
        public CommonExistsEntity(Func<string, string> quote, Func<string, string> buildParameter)
        {
            provider = new Optimized(quote, buildParameter);
        }

        Func<IDatabase, TEntity, bool> IOperationProvider<Func<IDatabase, TEntity, bool>>.Create(EntityDescriptor descriptor)
        {
            if (descriptor.Keys == null || descriptor.Keys.Length == 0)
                throw new InvalidOperationException("仅支持具备主键数据表的加载操作.");

            return provider.Create(descriptor);
        }

        #region Classes
        class Optimized : IOperationProvider<Func<IDatabase, TEntity, bool>>
        {
            private readonly Func<string, string> quote;
            private readonly Func<string, string> buildParameter;

            internal Optimized(Func<string, string> quote, Func<string, string> buildParameter)
            {
                this.quote = quote;
                this.buildParameter = buildParameter;
            }

            Func<IDatabase, TEntity, bool> IOperationProvider<Func<IDatabase, TEntity, bool>>.Create(EntityDescriptor descriptor)
            {

                var keys = descriptor.Keys.Select((key, index) => new MemberTuple<TEntity>
                        (
                            key.Name,
                            key.DbType,
                            buildParameter("p_sql_" + index),
                            Utils.MemberAccessor<TEntity, object>.GetGetter(key.Member)
                        )).ToArray();

                var sql = string.Concat("SELECT 1 FROM "
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
                            db.AddInParameter(command, key.ParameterName, key.DbType, key.Getter(entity));
                        }
                        return db.ExecuteScalar<int>(command) > 0;
                    }
                };
            }
        }

        class Normal : IOperationProvider<Func<IDatabase, TEntity, bool>>
        {
            Func<IDatabase, TEntity, bool> IOperationProvider<Func<IDatabase, TEntity, bool>>.Create(EntityDescriptor descriptor)
            {
                var keys = descriptor.Keys.Select((key, index) => new MemberTuple<TEntity>
                       (
                           key.Name,
                           key.DbType,
                           "p_sql_" + index,
                           Utils.MemberAccessor<TEntity, object>.GetGetter(key.Member)
                       )).ToArray();

                return (db, entity) =>
                {
                    var sql = string.Concat("SELECT 1 FROM "
                                            , string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.')
                                            , db.QuoteName(descriptor.Name)
                                            , " WHERE "
                                            , string.Join(", ", keys.Select(_ => db.QuoteName(_.Name) + " = " + db.BuildParameterName(_.ParameterName))));

                    using (var command = db.GetSqlStringCommand(sql))
                    {
                        foreach (var key in keys)
                        {
                            db.AddInParameter(command, db.BuildParameterName(key.ParameterName), key.DbType, key.Getter(entity));
                        }
                        return db.ExecuteScalar<int>(command) > 0;
                    }
                };
            }
        }
        #endregion
    }
}
