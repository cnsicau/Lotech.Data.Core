using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Lotech.Data.Providers
{
    /// <summary>
    /// 事务管理器提供者
    /// </summary>
    public interface ITransactionManagerProvider
    {
        /// <summary>
        /// 获取事务管理器
        /// </summary>
        /// <returns></returns>
        TransactionManager GetTransactionManager();

        /// <summary>
        /// 创建事务管理器
        /// </summary>
        /// <param name="requiresNew"></param>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        TransactionManager CreateTransactionManager(bool requiresNew, IsolationLevel isolationLevel);

        /// <summary>
        /// 创建事务管理器
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        TransactionManager CreateTransactionManager(IsolationLevel isolationLevel);

        /// <summary>
        /// 创建事务管理器
        /// </summary>
        /// <param name="requiresNew"></param>
        /// <returns></returns>
        TransactionManager CreateTransactionManager(bool requiresNew);

        /// <summary>
        /// 创建事务管理器
        /// </summary>
        /// <returns></returns>
        TransactionManager CreateTransactionManager();
    }
}
