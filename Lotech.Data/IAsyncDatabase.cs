using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Lotech.Data
{
    /// <summary>
    /// 
    /// </summary>
    public partial interface IDatabase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ConnectionSubstitute> GetConnectionAsync(DbCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DbDataReader> ExecuteReaderAsync(DbCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DataSet> ExecuteDataSetAsync(DbCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> ExecuteNonQueryAsync(DbCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<object> ExecuteScalarAsync(DbCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TScalar> ExecuteScalarAsync<TScalar>(DbCommand command, CancellationToken cancellationToken);
    }
}
