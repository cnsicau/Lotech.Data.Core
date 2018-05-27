using System.Threading;
using System.Threading.Tasks;

namespace Lotech.Data
{
    /// <summary>
    /// 连接提供程序
    /// </summary>
    public interface IConnectionProvider
    {
        /// <summary>
        /// 异步连接提供
        /// </summary>
        /// <returns></returns>
        Task<ConnectionSubstitute> GetConnectionAsync(CancellationToken token);
        

        /// <summary>
        /// 同步连接提供
        /// </summary>
        /// <returns></returns>
        ConnectionSubstitute GetConnection();
    }
}
