using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lotech.Data.Descriptors;
using Lotech.Data.Operations;
using Lotech.Data.Utils;

namespace Lotech.Data.SqlServers
{
    public class BulkCopy<TEntity> : IDisposable where TEntity : class
    {
        delegate void WriteToDelegate(SqlServerDatabase database, string destinationTableName, BulkCopyProvider provider, IEnumerable<TEntity> entities);

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

                    bulkCopy = Initialize();
                }

                BulkCopyDelegate Initialize()
                {
                    var bulkCopyMethod = new DynamicMethod("BulkCopy" + sqlBulkCopyType.MetadataToken.ToString("X")
                        , typeof(void), new Type[]
                        {
                            typeof(DbConnection),   //connection
                            typeof(DbTransaction),  //transaction
                            typeof(MemberTuple<TEntity>[]),  //columns
                            typeof(string),  //destinationTableName
                            typeof(BulkCopyDataReader<TEntity>),  //reader
                        }
                        , true);

                    var il = bulkCopyMethod.GetILGenerator();

                    // SqlBulkCopy(SqlConnection connection, SqlBulkCopyOptions copyOptions, SqlTransaction externalTransaction)
                    // SqlBulkCopy(SqlConnection connection);
                    var checkTransaction = il.DefineLabel();
                    var enter = il.DefineLabel();
                    var bulkCopy = il.DeclareLocal(sqlBulkCopyType);
                    var ctor3 = sqlBulkCopyType.GetConstructors().First(_ =>
                    {
                        var parameters = _.GetParameters();
                        return parameters.Length == 3
                            && parameters[0].ParameterType.IsSubclassOf(typeof(DbConnection))
                            && parameters[1].ParameterType == sqlBulkCopyOptionsType
                            && parameters[2].ParameterType.IsSubclassOf(typeof(DbTransaction));

                    });

                    var ctor1 = sqlBulkCopyType.GetConstructors().First(_ =>
                    {
                        var parameters = _.GetParameters();
                        return parameters.Length == 1 && parameters[0].ParameterType.IsSubclassOf(typeof(DbConnection));
                    });

                    il.Emit(OpCodes.Ldarg_0);  //connection
                    il.Emit(OpCodes.Castclass, ctor1.GetParameters()[0].ParameterType);

                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Brfalse_S, checkTransaction);

                    il.Emit(OpCodes.Ldc_I4_0); //options
                    il.Emit(OpCodes.Ldarg_1);  //transaction
                    il.Emit(OpCodes.Castclass, ctor3.GetParameters()[2].ParameterType);
                    il.Emit(OpCodes.Newobj, ctor3);
                    il.Emit(OpCodes.Br_S, enter);
                    il.MarkLabel(checkTransaction);

                    il.Emit(OpCodes.Newobj, ctor1);

                    il.MarkLabel(enter);
                    il.Emit(OpCodes.Stloc, bulkCopy);

                    // bulkCopy.DestinationTableName = destinationTableName;
                    il.Emit(OpCodes.Ldloc, bulkCopy);
                    il.Emit(OpCodes.Ldarg_3);
                    il.Emit(OpCodes.Call, destinationTableName.GetSetMethod());

                    // var index = columns.Length - 1;
                    var index = il.DeclareLocal(typeof(int));
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Ldlen);
                    il.Emit(OpCodes.Sub);
                    il.Emit(OpCodes.Stloc, index);

                    var columnMappings = il.DeclareLocal(this.columnMappings.PropertyType);
                    // var columnMappings = bulkCopy.ColumnMappings;
                    il.Emit(OpCodes.Ldloc, bulkCopy);
                    il.Emit(OpCodes.Call, this.columnMappings.GetGetMethod());
                    il.Emit(OpCodes.Stloc, columnMappings);

                    var loop = il.DefineLabel();
                    // while( index >= 0) 
                    il.Emit(OpCodes.Ldloc, index);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Blt_S, loop);
                    //    columnMappings.Add(index, columns[index].Name);
                    il.Emit(OpCodes.Ldloc, columnMappings);
                    il.Emit(OpCodes.Ldloc, index);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Ldloc, index);
                    il.Emit(OpCodes.Ldelem);
                    il.Emit(OpCodes.Call, typeof(MemberTuple<TEntity>).GetProperty("Name", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true));
                    il.Emit(OpCodes.Call, addColumnMapping);
                    // index --
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Ldloc, index);
                    il.Emit(OpCodes.Sub);
                    il.MarkLabel(loop);

                    il.Emit(OpCodes.Ldloc, bulkCopy);
                    il.Emit(OpCodes.Ldarg, 4);
                    il.Emit(OpCodes.Call, writeToServer);

                    il.Emit(OpCodes.Ret);

                    return (BulkCopyDelegate)bulkCopyMethod.CreateDelegate(typeof(BulkCopyDelegate));
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

                var columns = descriptor.Members.Where(_ => !_.DbGenerated)
                        .Select((_, i) => new MemberTuple<TEntity>(
                            _.Name,
                            _.DbType,
                            string.Empty,
                           MemberAccessor<TEntity, object>.GetGetter(_.Member)
                        )).ToArray();

                return (db, destinationTableName, provider, entities) =>
                {
                    using (var command = db.GetSqlStringCommand("SELECT 1"))
                    {
                        var connection = db.GetConnection(command);
                        if (connection.Connection.State == ConnectionState.Closed)
                            connection.Connection.Open();
                        if (db.Log != null) db.Log("BulkCopy to " + destinationTableName);
                        using (var reader = new BulkCopyDataReader<TEntity>(entities, columns))
                        {
                            provider.Instance()(connection.Connection, command.Transaction, columns, destinationTableName, reader);
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

        static public BulkCopy<TEntity> Create(SqlServerDatabase database, Operation operation)
        {
            var provider = BulkCopyProvider.Create(database.DbProviderFactory);
            if (provider == BulkCopyProvider.NotSupport) return null;

            var writeTo = Operation<TEntity, WriteToDelegate, BulkCopyOperationBuilder>.Instance(database.DescriptorProvider, operation);
            return new BulkCopy<TEntity>(database, provider, writeTo);

        }

        public void WriteTo(string destinationTableName, IEnumerable<TEntity> entities)
        {
            if (destinationTableName == null) throw new ArgumentNullException(nameof(destinationTableName));
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            writeTo(database, destinationTableName, provider, entities);
        }

        public void Dispose() { }
    }
}
