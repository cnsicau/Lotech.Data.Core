using System.Data.Common;
using Lotech.Data.SQLites;

namespace Lotech.Data
{
    using static SQLiteDatabase;
    /// <summary>
    /// 基于 SQLite 的库
    /// </summary>
    public class SQLiteConnectionDatabase : DbConnectionDatabase, IDatabase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        public SQLiteConnectionDatabase(DbConnection connection) : base(connection, new SQLiteEntityServices())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="services"></param>
        public SQLiteConnectionDatabase(DbConnection connection, IEntityServices services) : base(connection, services)
        {
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
    }
}
