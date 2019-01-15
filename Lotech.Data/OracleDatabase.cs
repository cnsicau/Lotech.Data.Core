using System;
using System.Data;
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
        public OracleDatabase(DbProviderFactory dbProviderFactory) : this(dbProviderFactory, new OracleEntityServices(dbProviderFactory)) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbProviderFactory"></param>
        /// <param name="services"></param>
        public OracleDatabase(DbProviderFactory dbProviderFactory, IEntityServices services) : base(dbProviderFactory, services)
        {
            this.DescriptorProvider = new UpperCaseDescriptorProvider();
        }

        internal static string Quote(string name) => string.Concat('"', name, '"');

        internal static string BuildParameter(string name) => string.Concat(':', name);

        /// <summary>
        /// 将 bool 转 Int16, 其他直接返回
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        internal static DbType FixDbType(DbType dbType)
        {
            if (dbType == DbType.Boolean) return DbType.Int16;
            return dbType;
        }

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

        /// <summary>
        /// 修复 boolean 绑定
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="direction"></param>
        /// <param name="value"></param>
        public override void AddParameter(DbCommand command, string parameterName, DbType dbType, ParameterDirection direction, object value)
        {
            base.AddParameter(command, parameterName, FixDbType(dbType), direction, value);
        }

        /// <summary>
        /// 修复 boolean 绑定
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        public override void AddOutParameter(DbCommand command, string parameterName, DbType dbType, int size)
        {
            base.AddOutParameter(command, parameterName, FixDbType(dbType), size);
        }

        /// <summary>
        /// 修复 boolean 绑定
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="direction"></param>
        /// <param name="size"></param>
        /// <param name="nullable"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="value"></param>
        public override void AddParameter(DbCommand command, string parameterName, DbType dbType, ParameterDirection direction, int size, bool nullable, int precision, int scale, object value)
        {
            base.AddParameter(command, parameterName, FixDbType(dbType), direction, size, nullable, precision, scale, value);
        }

        /// <summary>
        /// 特殊处理 boolean
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override DbType ParseDbType(Type type)
        {
            return FixDbType(base.ParseDbType(type));
        }
    }
}
