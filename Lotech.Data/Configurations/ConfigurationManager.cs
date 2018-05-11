using System;
using System.IO;

namespace Lotech.Data.Configurations
{
    /// <summary>
    /// 配置管理器
    /// </summary>
    static class ConfigurationManager
    {
        static DatabaseConfiguration configuration;

        static ConfigurationManager()
        {
            ConfigurationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.json");
        }

        /// <summary>
        /// 配置文件名
        /// </summary>
        static public string ConfigurationFile
        {
            get { return configuration?.FileName; }
            set
            {
                if (configuration?.FileName == value) return;
                configuration = new DatabaseConfiguration(value);
            }
        }

        /// <summary>
        /// 设置
        /// </summary>
        static public DatabaseSettings DatabaseSettings
        {
            get
            {
                return configuration?.DatabaseSettings;
            }
        }

        /// <summary>
        /// 连接串
        /// </summary>
        static public ConnectionStringSettingsCollection ConnectionStrings
        {
            get { return configuration?.ConnectionStrings; }
        }
    }
}
