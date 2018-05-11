using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.Common;

namespace Lotech.Data.Configurations
{
    /// <summary>
    /// 库配置
    /// </summary>
    class DatabaseConfiguration
    {
        /// <summary>
        /// 库设置配置节名称
        /// </summary>
        internal const string DatabaseSettingsName = "databaseSettings";
        
        /// <summary>
        /// 连接串配置节名称
        /// </summary>
        internal const string ConnectionStringsName = "connectionStrings";
        
        /// <summary>
        /// DB驱动工厂配置节名称
        /// </summary>
        internal const string DbProviderFactoriesName = "dbProviderFactories";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public DatabaseConfiguration(string fileName)
        {
            FileName = fileName;

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(fileName, false, false)
                .Build();

            var connections = new ConnectionStringSettingsCollection();
            configuration.Bind(ConnectionStringsName, connections);
            ConnectionStrings = connections;

            var settings = new DatabaseSettings();
            configuration.Bind(DatabaseSettingsName, settings);
            DatabaseSettings = settings;

            var providers = new Dictionary<string, string>();
            configuration.Bind(DbProviderFactoriesName, providers);

            foreach (var item in providers)
            {
                DbProviderFactories.RegisterFactory(item.Key, item.Value);
            }

            Configuration = configuration;
        }

        /// <summary>
        /// 连接串设置
        /// </summary>
        public ConnectionStringSettingsCollection ConnectionStrings { get; }

        /// <summary>
        /// 库设置
        /// </summary>
        public DatabaseSettings DatabaseSettings { get; }

        /// <summary>
        /// 配置
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// 配置文件名称
        /// </summary>
        public string FileName { get; }
    }

}
