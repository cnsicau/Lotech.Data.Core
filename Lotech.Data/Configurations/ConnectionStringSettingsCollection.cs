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
        public ConnectionStringSettingsCollection() : base(StringComparer.CurrentCultureIgnoreCase) { }
    }
}
