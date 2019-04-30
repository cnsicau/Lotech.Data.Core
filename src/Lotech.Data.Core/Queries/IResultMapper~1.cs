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
        /// <param name="database".
        /// <param name="record"></param>
        /// <returns></returns>
        object TearUp(IDatabase database, IDataRecord record);

        /// <summary>
        /// 映射结果
        /// </summary>
        /// <param name="record"></param>
        /// <param name="tearState"></param>
        /// <returns>映射的结果</returns>
        TResult Map(IDataRecord record, object tearState);

        /// <summary>
        /// 卸载状态
        /// </summary>
        /// <param name="tearState"></param>
        void TearDown(object tearState);
    }
}
