using System;

namespace Lotech.Data
{
    /// <summary>
    /// 实体读取器
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IEntityReader<TEntity> : IDisposable
    {
        /// <summary>
        /// 读取下一记录
        /// </summary>
        /// <returns></returns>
        bool Read();

        /// <summary>
        /// 获取当前值
        /// </summary>
        /// <returns></returns>
        TEntity GetValue();

        /// <summary>
        /// 关闭读取器
        /// </summary>
        void Close();
    }
}
