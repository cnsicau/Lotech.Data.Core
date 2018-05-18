using System.Data.Common;

namespace Lotech.Data
{
    /// <summary>
    /// 查询
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// 创建用于执行查询的 DbCommand
        /// </summary>
        /// <returns></returns>
        DbCommand CreateCommand();

        /// <summary>
        /// 获取该查询所属库
        /// </summary>
        IDatabase Database { get; }
    }
}
