namespace Lotech.Data.Configurations
{
    /// <summary>
    /// 库类型
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// 自动探测最优，未找到时使用通用
        /// </summary>
        Default = 0,
        /// <summary>
        /// 通用 DbProviderFactory 实例绑定的库
        /// </summary>
        Generic = 1,
        /// <summary>
        /// SqlServer库
        /// </summary>
        SqlServer = 2,
        /// <summary>
        /// MySql库
        /// </summary>
        MySql = 3,
        /// <summary>
        /// Oracle库
        /// </summary>
        Oracle = 4,
        /// <summary>
        /// SqlLite库
        /// </summary>
        SQLite = 5
    }
}
