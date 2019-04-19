using System.Data.Common;
using Lotech.Data.MySqls;

namespace Lotech.Data
{
    /// <summary>
    /// 基于 MySql 的库
    /// </summary>
    public class MySqlConnectionDatabase : DbConnectionDatabase, IDatabase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        public MySqlConnectionDatabase(DbConnection connection) : base(connection, new MySqlEntityServices())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="services"></param>
        public MySqlConnectionDatabase(DbConnection connection, IEntityServices services) : base(connection, services)
        {
        }

        /// <summary>
        /// 构建 :p_sql_0 格式的参数名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string BuildParameterName(string name) => MySqlEntityServices.BuildParameter(name);

        /// <summary>
        /// 构建 "NAME" 格式引用名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string QuoteName(string name) => MySqlEntityServices.Quote(name);
    }
}
