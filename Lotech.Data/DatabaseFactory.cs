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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IDatabase CreateDatabase()
        {
            var settings = ConfigurationManager.DatabaseSettings;
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
            var connectionSettings = ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionSettings == null)
                throw new InvalidOperationException("未找到默认连接: " + connectionName);

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
            IDatabase db;
            var provider = DbProviderFactories.GetFactory(connectionSettings.ProviderName);
            var databaseType = connectionSettings.Type;
            if (databaseType == DatabaseType.Default)
            {
                databaseType = providerDatabaseTypes.GetOrAdd(provider, DetectDatabaseType);
            }
            switch (databaseType)
            {
                case DatabaseType.Generic:
                    db = new GenericDatabase(provider
                            , connectionSettings.ParameterPrefix ?? "@"
                            , connectionSettings.QuoteName ?? "{0}");
                    break;
                case DatabaseType.SqlServer:
                    db = new SqlServerDatabase(provider);
                    break;
                case DatabaseType.Oracle:
                    db = new OracleDatabase(provider);
                    break;
                case DatabaseType.MySql:
                    db = new MySqlDatabase(provider);
                    break;
                case DatabaseType.SQLite:
                    db = new SQLiteDatabase(provider);
                    break;
                default:
                    throw new NotSupportedException("未支持的库类型");
            }

            db.ConnectionString = connectionSettings.ConnectionString;
            return db;
        }
    }
}
