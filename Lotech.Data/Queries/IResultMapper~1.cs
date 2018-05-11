using System.Data;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// DataReader结果映射器
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IResultMapper<TResult>
    {
        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="reader"></param>
        void TearUp(IResultSource reader);

        /// <summary>
        /// 映射下一结果
        /// </summary>
        /// <param name="result">映射的结果</param>
        /// <returns>返回映射成功与否</returns>
        bool MapNext(out TResult result);

        /// <summary>
        /// 拆毁
        /// </summary>
        void TearDown();
    }
}
