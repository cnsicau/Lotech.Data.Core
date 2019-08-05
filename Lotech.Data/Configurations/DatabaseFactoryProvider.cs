using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lotech.Data.Configurations
{

    /// <summary>
    /// 提升者
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class DatabaseFactoryProviderAttribute : Attribute
    {
        IDatabaseFactoryProvider provider;
        private readonly Type providerType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerType"></param>
        public DatabaseFactoryProviderAttribute(Type providerType)
        {
            if (!typeof(IDatabaseFactoryProvider).IsAssignableFrom(providerType))
            {
                throw new InvalidOperationException("providerType 必须实现 IDatabaseFactoryProvider");
            }
            if (providerType.IsAbstract)
                throw new InvalidOperationException("providerType 不能为抽象、静态类");
            this.providerType = providerType;
        }

        /// <summary>
        /// 级别默认为0， 越大优先处理
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IDatabaseFactoryProvider Provider
        {
            get
            {
                if (provider != null) return provider;
                return (provider = (IDatabaseFactoryProvider)Activator.CreateInstance(providerType));
            }
        }

        static readonly HashSet<DatabaseFactoryProviderAttribute> databaseFactoryProviderAttributes = new HashSet<DatabaseFactoryProviderAttribute>();

        static DatabaseFactoryProviderAttribute()
        {
            foreach (var frame in new StackTrace().GetFrames())
            {
                var method = frame.GetMethod();
                if (method == null || method.DeclaringType == null
                    || method.DeclaringType.Assembly == null || method.DeclaringType.Assembly.IsDynamic)
                    continue;

                var attributes = method.DeclaringType.Assembly.GetCustomAttributes(typeof(DatabaseFactoryProviderAttribute), false);
                if (attributes == null) continue;

                foreach (DatabaseFactoryProviderAttribute databaseFactoryProviderAttribute in attributes)
                {
                    databaseFactoryProviderAttributes.Add(databaseFactoryProviderAttribute);
                }
            }
        }

        internal static Func<IDatabase> CreateDatabaseFactory(System.Data.Common.DbProviderFactory provider, ConnectionStringSettings connectionStringSettings)
        {
            foreach (var attribute in databaseFactoryProviderAttributes.OrderByDescending(_ => _.Order))
            {
                var factory = attribute.Provider.Create(provider, connectionStringSettings);
                if (factory != null) return factory;
            }
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IDatabaseFactoryProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="connectionStringSettings"></param>
        /// <returns>如果支持构建，返回  Func, 否则返回 null</returns>
        Func<IDatabase> Create(System.Data.Common.DbProviderFactory factory, ConnectionStringSettings connectionStringSettings);
    }
}
