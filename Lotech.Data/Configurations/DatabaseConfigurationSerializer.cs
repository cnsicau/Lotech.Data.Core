using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Lotech.Data.Configurations
{
    /// <summary>
    /// 
    /// </summary>
    public class DatabaseConfigurationSerializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurationFile"></param>
        /// <returns></returns>
        public DatabaseConfiguration Parse(string configurationFile)
        {
            using (var stream = File.OpenRead(configurationFile))
            {
                var serializer = new XmlSerializer(typeof(Configuration));
                var configuration = (Configuration)serializer.Deserialize(stream);
                if (configuration?.DbProviderFactories != null)
                {
                    foreach (var registration in configuration.DbProviderFactories)
                        DbProviderFactories.RegisterFactory(registration.Name, registration.Type);
                }
                return new DatabaseConfiguration
                {
                    DatabaseSettings = new DatabaseSettings { DefaultDatabase = configuration?.Settings?.DefaultDatabase },
                    ConnectionStrings = new ConnectionStringSettingsCollection(
                        configuration.ConnectionStrings.ToDictionary(_ => _.Name, _ => new ConnectionStringSettings
                        {
                            ConnectionString = _.ConnectionString,
                            ParameterPrefix = _.ParameterPrefix,
                            ProviderName = _.ProviderName,
                            QuoteName = _.QuoteName,
                            Type = _.Type
                        })
                    )
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class DbProviderFactoryConfiguration
        {
            /// <summary>
            /// 
            /// </summary>
            [XmlAttribute("name")]
            public string Name { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [XmlAttribute("type")]
            public string Type { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ConnectionStringsConfiguration
        {
            /// <summary>
            /// 
            /// </summary>
            [XmlAttribute("name")]
            public string Name { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [XmlAttribute("providerName")]
            public string ProviderName { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [XmlAttribute("connectionString")]
            public string ConnectionString { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [XmlAttribute("type")]
            public DatabaseType Type { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [XmlAttribute("quoteName")]
            public string QuoteName { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [XmlAttribute("parameterPrefix")]
            public string ParameterPrefix { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class DatabaseSettingsConfiguration
        {
            /// <summary>
            /// 
            /// </summary>
            [XmlAttribute("defaultDatabase")]
            public string DefaultDatabase { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>

        [XmlRoot("database")]
        public class Configuration
        {
            [XmlArray("dbProviderFactories")]
            [XmlArrayItem("add")]
            public List<DbProviderFactoryConfiguration> DbProviderFactories { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [XmlArray("connectionStrings")]
            [XmlArrayItem("add")]
            public List<ConnectionStringsConfiguration> ConnectionStrings { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [XmlElement("databaseSettings")]
            public DatabaseSettingsConfiguration Settings { get; set; }
        }
    }
}
