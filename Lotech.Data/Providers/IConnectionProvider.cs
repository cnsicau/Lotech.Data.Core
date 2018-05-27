using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lotech.Data.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConnectionProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ConnectionSubstitute GetConnection();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ConnectionSubstitute> GetConnectionAsync(CancellationToken cancellationToken);
    }
}
