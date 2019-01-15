using Lotech.Data.Configurations;
using System;
using System.Collections.Concurrent;
using System.Data.Common;

namespace Lotech.Data
{
    /// <summary>
    /// Database 工厂
    /// </summary>
    public static class DatabaseFactory
    {
        static readonly ConcurrentDictionary<DbProviderFactory, DatabaseType> providerDatabaseTypes = new ConcurrentDictionary<DbProviderFactory, DatabaseType>();
        static readonly ConcurrentDictionary<ConnectionStringSettings, Func<IDatabase>> databaseProviders
            = new ConcurrentDictionary<ConnectionStringSettings, Func<IDatabase>>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IDatabase CreateDatabase()
        {
            var settings = DatabaseConfiguration.Current?.DatabaseSettings;
            if (settings == null)
                throw new InvalidProgramException("missing defaultDatabase");

            var defaultDatabase = settings?.DefaultDatabase;
            if (string.IsNullOrEmpty(defaultDatabase))
                throw new InvalidOperationException("未给出默认连接名.");

            return CreateDatabase(defaultDatabase);
        }

        /// <summary>
        /// 基于连接名创建库
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public static IDatabase CreateDatabase(string connectionName)
        {
            var connectionSettings = DatabaseConfiguration.Current?.ConnectionStrings[connectionName];
            if (connectionSettings == null)
                throw new InvalidOperationException("未找到默认连接: " + connectionName);
            if (connectionSettings.ProviderName == null)
                throw new InvalidOperationException($"连接{connectionName}的 providerName必须指出");
            if (connectionSettings.ConnectionString == null)
                throw new InvalidOperationException($"连接{connectionName}的 connectionString必须指出");

            return CreateDatabase(connectionSettings);
        }

        private static DatabaseType DetectDatabaseType(DbProviderFactory factory)
        {
            switch (factory.GetType().Name.ToLower())
            {
                // OracleClientFactory
                case "oracleclientfactory": return DatabaseType.Oracle;
                // SqlClientFactory
                case "sqlclientfactory": return DatabaseType.SqlServer;
                // MySqlClientFactory
                case "mysqlclientfactory": return DatabaseType.MySql;
                // SQLiteFactory
                case "sqlitefactory": return DatabaseType.SQLite;
                default: return DatabaseType.Default;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionSettings"></param>
        /// <returns></returns>
        public static IDatabase CreateDatabase(ConnectionStringSettings connectionSettings)
        {
            var db = databaseProviders.GetOrAdd(connectionSettings, CreateDatabaseProvider)();
            db.ConnectionString = connectionSettings.ConnectionString;
            return db;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionSettings"></param>
        /// <returns></returns>
        public static Func<IDatabase> CreateDatabaseProvider(ConnectionStringSettings connectionSettings)
        {
            var provider = Configurations.DbProviderFactories.GetFactory(connectionSettings.ProviderName);
            var databaseType = connectionSettings.Type;
            if (databaseType == DatabaseType.Default)
            {
                databaseType = providerDatabaseTypes.GetOrAdd(provider, DetectDatabaseType);
            }
            var connectionString = connectionSettings.ConnectionString;
            switch (databaseType)
            {
#if ALL || GENERIC
                case DatabaseType.Generic:
                    var parameter = connectionSettings.ParameterPrefix ?? "@";
                    var quote = connectionSettings.QuoteName ?? "{0}";
                    return () => new GenericDatabase(provider, parameter, quote) { ConnectionString = connectionString };
#endif
#if ALL || SQLSERVER
                case DatabaseType.SqlServer:
                    return () => new SqlServerDatabase(provider);
#endif
#if ALL || ORACLE
                case DatabaseType.Oracle:
                    return () => new OracleDatabase(provider);
#endif
#if ALL || MYSQL
                case DatabaseType.MySql:
                    return () => new MySqlDatabase(provider);
#endif
#if ALL || SQLITE
                case DatabaseType.SQLite:
                    return () => new SQLiteDatabase(provider);
#endif
                default:
                    return () => { throw new NotSupportedException("未支持的库类型"); };
            }
        }
    }
}
