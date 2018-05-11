using System.Data.Common;
using Lotech.Data.Oracles;

namespace Lotech.Data
{
    /// <summary>
    /// 基于 Oracle 的库
    /// </summary>
    public class OracleDatabase : DbProviderDatabase, IDatabase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbProviderFactory"></param>
        public OracleDatabase(DbProviderFactory dbProviderFactory) : base(dbProviderFactory, new OracleEntityServices())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbProviderFactory"></param>
        /// <param name="services"></param>
        public OracleDatabase(DbProviderFactory dbProviderFactory, IEntityServices services) : base(dbProviderFactory, services)
        {
        }

        internal static string Quote(string name) => string.Concat('"', name, '"');

        internal static string BuildParameter(string name) => string.Concat(':', name);

        /// <summary>
        /// 构建 :p_sql_0 格式的参数名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string BuildParameterName(string name) => BuildParameter(name);

        /// <summary>
        /// 构建 "NAME" 格式引用名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string QuoteName(string name) => Quote(name);
    }
}
