using System;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 结果数据源
    /// </summary>
    public interface IResultSource : IDisposable
    {
        /// <summary>
        /// 获取所有列数量
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        /// 获取指定列名称
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        string GetColumnName(int index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        object GetColumnValue(int index);

        /// <summary>
        /// 读取下一行
        /// </summary>
        /// <returns></returns>
        bool Next();

        /// <summary>
        /// 取指定列的类型
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        Type GetColumnType(int columnIndex);
    }
}
