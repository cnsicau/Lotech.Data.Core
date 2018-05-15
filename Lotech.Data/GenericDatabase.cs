using Lotech.Data.Generics;
using Lotech.Data.Utils;
using System.Data.Common;
using System.Diagnostics;

namespace Lotech.Data
{
    /// <summary>
    /// 通用Database实现
    /// </summary>
    public class GenericDatabase : DbProviderDatabase
    {
        private readonly string _parameterPrefix;
        private readonly string _quoteFormat;


        /// <summary>
        /// 构造通用库
        /// </summary>
        /// <param name="dbProviderFactory">DbProvider实例</param>
        public GenericDatabase(DbProviderFactory dbProviderFactory) : this(dbProviderFactory, "@", "{0}") { }

        /// <summary>
        /// 构造通用库
        /// </summary>
        /// <param name="dbProviderFactory">DbProvider实例</param>
        /// <param name="parameterPrefix">参数前缀</param>
        public GenericDatabase(DbProviderFactory dbProviderFactory
            , string parameterPrefix) : this(dbProviderFactory, parameterPrefix, "{0}") { }

        /// <summary>
        /// 构造通用库
        /// </summary>
        /// <param name="dbProviderFactory">DbProvider实例</param>
        /// <param name="parameterPrefix">参数前缀</param>
        /// <param name="quoteFormat">引述格式串</param>
        public GenericDatabase(DbProviderFactory dbProviderFactory
            , string parameterPrefix
            , string quoteFormat)
            : base(dbProviderFactory, new GenericEntityServices())
        {
            _parameterPrefix = parameterPrefix;
            _quoteFormat = quoteFormat;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string QuoteName(string name)
        {
            return string.Format(NameFormatProvider.Instance, _quoteFormat, name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string BuildParameterName(string name)
        {
            return _parameterPrefix + name;
        }
    }
}