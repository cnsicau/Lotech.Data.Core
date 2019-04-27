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
        void Initialize(IDatabase database, IDataRecord record);

        /// <summary>
        /// 映射结果
        /// </summary>
        /// <param name="record"></param>
        /// <returns>映射的结果</returns>
        TResult Map(IDataRecord record);
    }
}
