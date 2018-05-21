﻿using System;
using System.IO;

namespace Lotech.Data.Configurations
{
    /// <summary>
    /// 配置
    /// </summary>
    public class DatabaseConfiguration
    {
        static readonly string ConfigurationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.xml");
        static readonly Lazy<DatabaseConfiguration> configuration
                = new Lazy<DatabaseConfiguration>(() => new DatabaseConfigurationSerializer().Parse(ConfigurationFile));

        /// <summary>
        /// 获取当前设置
        /// </summary>
        static public DatabaseConfiguration Current { get { return configuration.Value; } }

        /// <summary>
        /// 
        /// </summary>
        public DatabaseConfiguration() { }

        /// <summary>
        /// 连接串设置
        /// </summary>
        public ConnectionStringSettingsCollection ConnectionStrings { get; set; }

        /// <summary>
        /// 库设置
        /// </summary>
        public DatabaseSettings DatabaseSettings { get; set; }
    }
}
