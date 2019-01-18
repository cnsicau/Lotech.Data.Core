using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lotech.Data.Descriptors;
using Lotech.Data.Operations;
using Lotech.Data.Utils;

namespace Lotech.Data.SqlServers
{
    class BulkCopy<TEntity> : IDisposable where TEntity : class
    {
        delegate void WriteToDelegate(SqlServerDatabase database, string destinationTableName, BulkCopyProvider provider, IEnumerable<TEntity> entities, Func<MemberTuple<TEntity>, bool> columnFilter);

        delegate void BulkCopyDelegate(DbConnection connection, DbTransaction transaction, MemberTuple<TEntity>[] columns, string destinationTableName, BulkCopyDataReader<TEntity> reader);
        #region Static members

        class BulkCopyProvider
        {
            static readonly ConcurrentDictionary<DbProviderFactory, BulkCopyProvider>
                providers = new ConcurrentDictionary<DbProviderFactory, BulkCopyProvider>();

            internal static readonly BulkCopyProvider NotSupport = new BulkCopyProvider();

            class BulkCopyProviderImpl : BulkCopyProvider
            {

                private readonly Type sqlBulkCopyType;
                private readonly Type sqlBulkCopyOptionsType;
                private readonly PropertyInfo destinationTableName;
                private readonly PropertyInfo columnMappings;
                private readonly MethodInfo addColumnMapping;
                private readonly BulkCopyDelegate bulkCopy;
                private MethodInfo writeToServer;

                public BulkCopyProviderImpl(Type sqlBulkCopyType, Type sqlBulkCopyOptionsType, PropertyInfo destinationTableName, PropertyInfo columnMappings, MethodInfo addColumnMapping, MethodInfo writeToServer)
                {
                    this.sqlBulkCopyType = sqlBulkCopyType;
                    this.sqlBulkCopyOptionsType = sqlBulkCopyOptionsType;
                    this.destinationTableName = destinationTableName;
                    this.columnMappings = columnMappings;
                    this.addColumnMapping = addColumnMapping;
                    this.writeToServer = writeToServer;

                    bulkCopy = CompileBulkCopy();
                }

                BulkCopyDelegate CompileBulkCopy()
                {
                    var connection = Expression.Parameter(typeof(DbConnection));
                    var transaction = Expression.Parameter(typeof(DbTransaction));
                    var columns = Expression.Parameter(typeof(MemberTuple<TEntity>[]));
                    var destinationTableName = Expression.Parameter(typeof(string));
                    var reader = Expression.Parameter(typeof(BulkCopyDataReader<TEntity>));

                    var bulkCopy = Expression.Variable(sqlBulkCopyType);
                    var index = Expression.Variable(typeof(int));
                    var loopLabel = Expression.Label();
                    var returnLabel = Expression.Label();

                    #region Constructor
                    Type sqlConnectionType = null, sqlTransactionType = null;

                    var transactionalConstructor = sqlBulkCopyType.GetConstructors().First(_ =>
                    {
                        var parameters = _.GetParameters();
                        return parameters.Length == 3
                            && (sqlConnectionType = parameters[0].ParameterType).IsSubclassOf(typeof(DbConnection))
                            && parameters[1].ParameterType == sqlBulkCopyOptionsType
                            && (sqlTransactionType = parameters[2].ParameterType).IsSubclassOf(typeof(DbTransaction));

                    });

                    var connectionConstructor = sqlBulkCopyType.GetConstructors().First(_ =>
                    {
                        var parameters = _.GetParameters();
                        return parameters.Length == 1 && parameters[0].ParameterType.IsSubclassOf(typeof(DbConnection));
                    });

                    #endregion

                    var block = Expression.Block(
                        new ParameterExpression[] { bulkCopy, index },
                        // var bulkCopy = transaction == null 
                        //      ? new SqlBulkCopy(connection)
                        //      : new SqlBulkCopy(connection, 0, transaction);
                        Expression.Assign(
                                bulkCopy, Expression.Condition(
                                        Expression.Equal(transaction, Expression.Constant(null))
                                        , Expression.New(connectionConstructor, Expression.Convert(connection, sqlConnectionType))
                                        , Expression.New(transactionalConstructor, Expression.Convert(connection, sqlConnectionType)
                                                , Expression.Convert(Expression.Constant(0), sqlBulkCopyOptionsType)
                                                , Expression.Convert(transaction, sqlTransactionType))
                                    )
                        ),

                        // bulkCopy.DestinationTableName = destinationTableName;
                        Expression.Assign(
                            Expression.MakeMemberAccess(bulkCopy, this.destinationTableName), destinationTableName),

                        // var index = columns.Length;
                        Expression.Assign(
                                index, Expression.ArrayLength(columns)
                            ),
                        // while(index >= 0) bulkCopy.ColumnMappings.Add(index, columns[index].Name);
                        Expression.Loop(
                                Expression.IfThenElse(
                                        Expression.LessThan(
                                            Expression.Assign(index, Expression.Decrement(index))
                                            , Expression.Constant(0, typeof(int))),
                                        Expression.Break(loopLabel),
                                        Expression.Call(
                                                Expression.MakeMemberAccess(bulkCopy, this.columnMappings),
                                                addColumnMapping,
                                                index,
                                                Expression.MakeMemberAccess(
                                                        Expression.ArrayIndex(columns, index),
                                                        typeof(MemberTuple<TEntity>).GetProperty("Name", BindingFlags.NonPublic | BindingFlags.Instance)
                                                    )
                                            )
                                    )
                                , loopLabel
                            ),
                        //// bulkCopy.WriteTo(reader);
                        Expression.Call(bulkCopy, writeToServer, reader),
                        Expression.Return(returnLabel),
                        Expression.Label(returnLabel)
                    );

                    return Expression.Lambda<BulkCopyDelegate>(
                            block,
                            connection, transaction, columns, destinationTableName, reader
                        )
                        .Compile();
                }

                public override BulkCopyDelegate Instance()
                {
                    return bulkCopy;
                }
            }

            public virtual BulkCopyDelegate Instance() { throw new NotSupportedException(); }

            private BulkCopyProvider() { }

            public static BulkCopyProvider Create(DbProviderFactory dbProviderFactory)
            {
                return providers.GetOrAdd(dbProviderFactory, CreateBulkProvider);
            }

            private static BulkCopyProvider CreateBulkProvider(DbProviderFactory factory)
            {
                var factorType = factory.GetType();
                var sqlBulkCopyType = Type.GetType(factorType.AssemblyQualifiedName.Replace(factorType.Name, "SqlBulkCopy"), false);
                if (sqlBulkCopyType == null) return NotSupport;

                var writeToServer = sqlBulkCopyType.GetMethod("WriteToServer", new Type[] { typeof(IDataReader) });
                if (writeToServer == null) return NotSupport;

                var sqlBulkCopyOptionsType = Type.GetType(factorType.AssemblyQualifiedName.Replace(factorType.Name, "SqlBulkCopyOptions"), false);
                if (sqlBulkCopyOptionsType == null) return NotSupport;

                var destinationTableName = sqlBulkCopyType.GetProperty("DestinationTableName");
                if (destinationTableName == null) return NotSupport;

                var columnMappings = sqlBulkCopyType.GetProperty("ColumnMappings");
                if (columnMappings == null) return NotSupport;

                var addColumnMapping = columnMappings.PropertyType.GetMethod("Add", new Type[] { typeof(int), typeof(string) });
                if (columnMappings == null) return NotSupport;

                return new BulkCopyProviderImpl(sqlBulkCopyType, sqlBulkCopyOptionsType, destinationTableName, columnMappings, addColumnMapping, writeToServer);
            }
        }

        class BulkCopyOperationBuilder : IOperationProvider<WriteToDelegate>
        {
            public WriteToDelegate Create(IEntityDescriptor descriptor)
            {
                if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
                if (descriptor.Type != typeof(TEntity)) throw new InvalidOperationException("实体描述符与当前类型不匹配.");

                var members = descriptor.Members.Where(_ => !_.DbGenerated)
                        .Select((_, i) => new MemberTuple<TEntity>(
                            _.Name,
                            _.DbType,
                            string.Empty,
                           MemberAccessor<TEntity, object>.GetGetter(_.Member)
                        )).ToArray();

                return (db, destinationTableName, provider, entities, columnFilter) =>
                {
                    using (var command = db.GetSqlStringCommand("SELECT 1"))
                    {
                        var connection = db.GetConnection(command);
                        if (connection.Connection.State == ConnectionState.Closed)
                            connection.Connection.Open();
                        if (db.Log != null) db.Log("BulkCopy to " + destinationTableName);
                        var columns = columnFilter == null ? members : (members.Where(columnFilter).ToArray());
                        using (var reader = new BulkCopyDataReader<TEntity>(entities, columns))
                        {
                            provider.Instance()(connection.Connection, command.Transaction
                                , columns
                                , destinationTableName, reader);
                        }
                    }
                };
            }
        }

        #endregion

        private readonly SqlServerDatabase database;
        private readonly BulkCopyProvider provider;
        private readonly WriteToDelegate writeTo;

        BulkCopy(SqlServerDatabase database, BulkCopyProvider provider, WriteToDelegate writeTo)
        {
            this.database = database;
            this.provider = provider;
            this.writeTo = writeTo;
        }

        static internal BulkCopy<TEntity> Create(SqlServerDatabase database, Operation operation)
        {
            var provider = BulkCopyProvider.Create(database.DbProviderFactory);
            if (provider == BulkCopyProvider.NotSupport) return null;

            var writeTo = Operation<TEntity, WriteToDelegate, BulkCopyOperationBuilder>.Instance(database.DescriptorProvider, operation);
            return new BulkCopy<TEntity>(database, provider, writeTo);

        }

        internal void WriteTo(string destinationTableName, IEnumerable<TEntity> entities, Func<MemberTuple<TEntity>, bool> columnFilter = null)
        {
            if (destinationTableName == null) throw new ArgumentNullException(nameof(destinationTableName));
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            writeTo(database, destinationTableName, provider, entities, columnFilter);
        }

        void IDisposable.Dispose() { }
    }
}
