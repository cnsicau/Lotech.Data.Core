
namespace Lotech.Data.Configurations
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    public class DatabaseSettings
    {
        /// <summary>
        /// 默认数据库连接名称
        /// </summary>
        public string DefaultDatabase { get; set; }

        /// <summary>
        /// 是否启用 Trace 输出
        /// </summary>
        public bool Trace { get; set; }
    }
}
