using System.Collections.Generic;

namespace Lotech.Data.Configurations
{
    /// <summary>
    /// 连接串设置
    /// </summary>
    public class ConnectionStringSettings
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public ConnectionStringSettings(IReadOnlyDictionary<string, string> items) { Items = items; }

        /// <summary>
        /// 额外属性
        /// </summary>
        public IReadOnlyDictionary<string, string> Items { get; }

        /// <summary>
        /// 提供者名称，如：System.Data.SqlClient
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 获取此连接串对应的库类型 Default\Generic\SqlServer\MySql\Oracle\SQLite
        /// </summary>
        public DatabaseType Type { get; set; }

        /// <summary>
        /// Generic DB 参数化器
        /// </summary>
        public string ParameterPrefix { get; set; }

        /// <summary>
        /// Generic DB 名称引用
        /// </summary>
        public string QuoteName { get; set; }
    }

}
