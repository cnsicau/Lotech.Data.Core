using System;
using System.Collections.Generic;
using System.Data;

namespace Lotech.Data.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public class TransactionManagerProvider : ITransactionManagerProvider
    {
        /// <summary>
        /// 默认实例
        /// </summary>
        public static readonly ITransactionManagerProvider Default = new TransactionManagerProvider();

        [ThreadStatic]
        static Stack<TransactionManager> transactionManagers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requiresNew"></param>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        public virtual TransactionManager CreateTransactionManager(bool requiresNew, IsolationLevel isolationLevel)
        {
            return AddTransactionManager(new TransactionManager(requiresNew, isolationLevel, this));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        public virtual TransactionManager CreateTransactionManager(IsolationLevel isolationLevel)
        {
            return AddTransactionManager(new TransactionManager(isolationLevel, this));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requiresNew"></param>
        /// <returns></returns>
        public virtual TransactionManager CreateTransactionManager(bool requiresNew)
        {
            return AddTransactionManager(new TransactionManager(requiresNew, this));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual TransactionManager CreateTransactionManager()
        {
            var transactionManager = GetTransactionManager();
            return transactionManager ?? AddTransactionManager(new TransactionManager(this));
        }

        TransactionManager AddTransactionManager(TransactionManager transactionManager)
        {
            transactionManager.Completed += (s, e) =>
            {
                transactionManagers.Pop();
                if (transactionManagers.Count == 0)
                    transactionManagers = null;
            };

            if (transactionManagers == null || transactionManagers.Count == 0)
            {
                transactionManagers = new Stack<TransactionManager>(new[] { transactionManager });
            }
            else
            {
                transactionManagers.Push(transactionManager);
            }

            return transactionManager;
        }

        /// <summary>
        /// 获取当前事务管理器
        /// </summary>
        /// <returns></returns>
        public virtual TransactionManager GetTransactionManager()
        {
            return transactionManagers?.Count > 0 ? transactionManagers.Peek() : null;
        }
    }
}
