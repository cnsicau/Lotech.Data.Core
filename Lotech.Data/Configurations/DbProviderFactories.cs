using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace Lotech.Data.Configurations
{
    /// <summary>DB驱动配置</summary>
    static public class DbProviderFactories
    {
        static readonly Dictionary<string, DbProviderFactory> factories = new Dictionary<string, DbProviderFactory>(StringComparer.CurrentCultureIgnoreCase);

#if !DOTNET_CORE && NET_4
        static DbProviderFactories()
        {
            // 默认注册 SqlServer、Odbc、Oledb 驱动
            factories.Add("System.Data.SqlClient", System.Data.SqlClient.SqlClientFactory.Instance);
            factories.Add("System.Data.Odbc", System.Data.Odbc.OdbcFactory.Instance);
            factories.Add("System.Data.OleDb", System.Data.OleDb.OleDbFactory.Instance);
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factory"></param>
        static public void RegisterFactory(string name, DbProviderFactory factory)
        {
            factories[name] = factory;
        }

        ///注册驱动
        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="providerFactorTypeName"></param>
        static public void RegisterFactory(string name, string providerFactorTypeName)
        {
            var provider = Type.GetType(providerFactorTypeName, false);
            if (provider == null) throw new InvalidOperationException("DbProviderFactory not found: " + providerFactorTypeName);

            factories[name] = (DbProviderFactory)provider.InvokeMember("Instance", BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic, null, null, null);
        }

        /// <summary>
        /// 获取配置的DB驱动
        /// </summary>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static DbProviderFactory GetFactory(string providerName)
        {
            DbProviderFactory factory;
            if (!factories.TryGetValue(providerName, out factory))
                throw new KeyNotFoundException("DbProviderFactory is not found: " + providerName);
            return factory;
        }
    }
}