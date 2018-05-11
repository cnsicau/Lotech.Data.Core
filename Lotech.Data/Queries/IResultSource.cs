using System;
using System.Collections.Generic;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 结果数据源
    /// </summary>
    public interface IResultSource : IDisposable
    {
        /// <summary>
        /// 获取所有列
        /// </summary>
        IList<string> Columns { get; }

        /// <summary>
        /// 获取指定位置的当前数据
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        object this[int columnIndex] { get; }

        /// <summary>
        /// 获取当前行所有对象
        /// </summary>
        /// <returns></returns>
        object[] GetRow();

        /// <summary>
        /// 读取下一行
        /// </summary>
        /// <returns></returns>
        bool Next();

        /// <summary>
        /// 取指定列的类型
        /// </summary>
        /// <param name="columnIndexi"></param>
        /// <returns></returns>
        Type GetColumnType(int columnIndexi);
    }
}
