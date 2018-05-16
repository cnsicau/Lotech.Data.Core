using System;
using System.Collections.Generic;

namespace Lotech.Data.Configurations
{
    /// <summary>
    /// 连接设置集合
    /// </summary>
    public class ConnectionStringSettingsCollection : Dictionary<string, ConnectionStringSettings>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionSettings"></param>
        public ConnectionStringSettingsCollection(IDictionary<string, ConnectionStringSettings> connectionSettings)
            : base(connectionSettings) { }
        /// <summary>
        /// 
        /// </summary>
        public ConnectionStringSettingsCollection() : base(StringComparer.CurrentCultureIgnoreCase) { }
    }
}
