using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
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
            if (!File.Exists(configurationFile))
            {
                Trace.TraceWarning("未找到配置文件 " + configurationFile + ", 使用空配置");
                return new DatabaseConfiguration
                {
                    ConnectionStrings = new ConnectionStringSettingsCollection(),
                };
            }
            using (var stream = File.OpenRead(configurationFile))
            {
                var reader = XmlReader.Create(stream);
                string xmlns = reader.MoveToContent() == XmlNodeType.Element
                    && reader.IsStartElement("database")
                    && reader.MoveToAttribute("xmlns")
                    && reader.ReadAttributeValue() ? reader.Value : string.Empty;

                stream.Seek(0, SeekOrigin.Begin);

                var serializer = new XmlSerializer(typeof(Configuration), xmlns);
                serializer.UnknownAttribute += AppendConnectionStringsConfigurationItems;
                var configuration = (Configuration)serializer.Deserialize(stream);
                if (configuration?.DbProviderFactories != null)
                {
                    foreach (var registration in configuration.DbProviderFactories)
                        DbProviderFactories.RegisterFactory(registration.Name, registration.Type);
                }
                return new DatabaseConfiguration
                {
                    DatabaseSettings = new DatabaseSettings
                    {
                        DefaultDatabase = configuration?.Settings?.DefaultDatabase,
                        Trace = configuration?.Settings?.Trace ?? false
                    },
                    ConnectionStrings = new ConnectionStringSettingsCollection(
                        configuration.ConnectionStrings.ToDictionary(_ => _.Name ?? string.Empty, _ => new ConnectionStringSettings(_.Properties)
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

        private void AppendConnectionStringsConfigurationItems(object sender, XmlAttributeEventArgs e)
        {
            if (e.ObjectBeingDeserialized is ConnectionStringsConfiguration)
            {
                ((ConnectionStringsConfiguration)e.ObjectBeingDeserialized).Properties.Add(
                    e.Attr.Name,
                    e.Attr.Value
                );
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
            public ConnectionStringsConfiguration()
            {
                Properties = new Dictionary<string, string>();
            }

            /// <summary>
            /// 连接串属性
            /// </summary>
            [XmlIgnore]
            public Dictionary<string, string> Properties { get; }

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
            /// <summary>
            /// 
            /// </summary>
            [XmlAttribute("trace")]
            public bool Trace { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>

        [XmlRoot("database")]
        public class Configuration
        {
            /// <summary>
            /// 
            /// </summary>
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
