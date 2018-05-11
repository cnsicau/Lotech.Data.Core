using System.Data.Common;
using Lotech.Data.SqlServers;

namespace Lotech.Data
{
    /// <summary>
    /// 基于 SqlServer 的库
    /// </summary>
    public class SqlServerDatabase : DbProviderDatabase, IDatabase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbProviderFactory"></param>
        public SqlServerDatabase(DbProviderFactory dbProviderFactory) : base(dbProviderFactory, new SqlServerEntityServices())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbProviderFactory"></param>
        /// <param name="services"></param>
        public SqlServerDatabase(DbProviderFactory dbProviderFactory, IEntityServices services) : base(dbProviderFactory, services)
        {
        }

        internal static string Quote(string name) => string.Concat('[', name, ']');

        internal static string BuildParameter(string name) => string.Concat('@', name);

        /// <summary>
        /// 构建 @p_sql_0 格式的参数名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string BuildParameterName(string name) => BuildParameter(name);

        /// <summary>
        /// 构建 [Name] 格式引用名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string QuoteName(string name) => Quote(name);
    }
}
