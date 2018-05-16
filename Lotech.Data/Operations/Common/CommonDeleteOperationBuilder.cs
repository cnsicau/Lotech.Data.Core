using Lotech.Data.Descriptors;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Lotech.Data.Operations.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class CommonDeleteOperationBuilder<TEntity> : IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>
        where TEntity : class
    {
        IOperationBuilder<Action<IDatabase, DbCommand, TEntity>> builder;

        /// <summary>
        /// 
        /// </summary>
        protected CommonDeleteOperationBuilder() { builder = new Normal(); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="buildParameter"></param>
        protected CommonDeleteOperationBuilder(Func<string, string> quote, Func<string, string> buildParameter)
        {
            builder = new Optmized(quote, buildParameter);
        }

        /// <summary>
        /// 基于Database在调用时构建SQL
        /// </summary>
        class Normal : IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>
        {
            Func<IDatabase, DbCommand> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildCommandProvider(EntityDescriptor descriptor)
            {
                if (descriptor.Keys == null || descriptor.Keys.Length == 0)
                    throw new InvalidOperationException("仅支持具备主键数据表的删除操作.");

                var keys = descriptor.Keys.Select((key, index) => new MemberTuple<TEntity>(key.Name, "p_sql_" + index)).ToArray();
                return db =>
                {
                    var sql = string.Concat("DELETE FROM "
                                                , string.IsNullOrEmpty(descriptor.Schema) ? null : (db.QuoteName(descriptor.Schema) + '.')
                                                , db.QuoteName(descriptor.Name)
                                                , " WHERE "
                                                , string.Join(", ", keys.Select(_ => db.QuoteName(_.Name) + " = " + db.BuildParameterName(_.ParameterName))));

                    return db.GetSqlStringCommand(sql);
                };
            }

            Action<IDatabase, DbCommand, TEntity> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildInvoker(EntityDescriptor descriptor)
            {
                var keys = descriptor.Keys.Select((key, index) => new MemberTuple<TEntity>(key.Name,
                            key.DbType,
                             "p_sql_" + index,
                             Utils.MemberAccessor<TEntity, object>.GetGetter(key.Member)
                        )).ToArray();

                return (db, command, entity) =>
                {
                    foreach (var key in keys)
                    {
                        db.AddInParameter(command, db.BuildParameterName(key.ParameterName), key.DbType, key.Getter(entity));
                    }
                    db.ExecuteNonQuery(command);
                };
            }
        }

        /// <summary>
        /// 基于预处理构建SQL，并在调用时直接使用SQL
        /// </summary>
        class Optmized : IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>
        {
            Func<string, string> quote;
            Func<string, string> buildParameter;
            internal Optmized(Func<string, string> quote, Func<string, string> buildParameter)
            {
                this.quote = quote;
                this.buildParameter = buildParameter;
            }

            string BuilderParameterName(int index) { return buildParameter("p_sql_" + index); }


            public Func<IDatabase, DbCommand> BuildCommandProvider(EntityDescriptor descriptor)
            {
                if (descriptor.Keys == null || descriptor.Keys.Length == 0)
                    throw new InvalidOperationException("仅支持具备主键数据表的删除操作.");

                var keys = descriptor.Keys
                        .Select((key, index) => new MemberTuple<TEntity>(key.Name, BuilderParameterName(index))).ToArray();

                var sql = string.Concat("DELETE FROM "
                                        , string.IsNullOrEmpty(descriptor.Schema) ? null : (quote(descriptor.Schema) + '.')
                                        , quote(descriptor.Name)
                                        , " WHERE "
                                        , string.Join(", ", keys.Select(_ => quote(_.Name) + " = " + _.ParameterName)));

                return db => db.GetSqlStringCommand(sql);
            }

            public Action<IDatabase, DbCommand, TEntity> BuildInvoker(EntityDescriptor descriptor)
            {
                var keys = descriptor.Keys.Select((key, index) => new MemberTuple<TEntity>(key.Name,
                                key.DbType,
                                BuilderParameterName(index),
                                Utils.MemberAccessor<TEntity, object>.GetGetter(key.Member)
                           )).ToArray();

                return (db, command, entity) =>
                {
                    foreach (var key in keys)
                    {
                        db.AddInParameter(command, key.ParameterName, key.DbType, key.Getter(entity));
                    }
                    db.ExecuteNonQuery(command);
                };
            }
        }


        Func<IDatabase, DbCommand> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildCommandProvider(EntityDescriptor descriptor)
        {
            return builder.BuildCommandProvider(descriptor);
        }

        Action<IDatabase, DbCommand, TEntity> IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>.BuildInvoker(EntityDescriptor descriptor)
        {
            return builder.BuildInvoker(descriptor);
        }
    }
}
