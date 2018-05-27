using Lotech.Data.Providers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lotech.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class TransactionManager : IDisposable
    {
        #region Fields

        private readonly Guid id = Guid.NewGuid();
        private readonly Dictionary<string, DbTransaction> transactions = new Dictionary<string, DbTransaction>();
        private readonly TransactionManager parentManager;
        private readonly IsolationLevel? isolationLevel;
        #endregion

        #region Event && Properties
        /// <summary>
        /// 完成
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// 父级管理器
        /// </summary>
        public TransactionManager Parent { get { return parentManager; } }

        /// <summary>
        /// 隔离级别
        /// </summary>
        public IsolationLevel IsolationLevel { get { return isolationLevel ?? (IsolationLevel.Unspecified); } }
        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        public TransactionManager(ITransactionManagerProvider provider) : this(false, null, provider) { }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <param name="provider"></param>
        public TransactionManager(IsolationLevel isolationLevel, ITransactionManagerProvider provider) : this(false, (IsolationLevel?)isolationLevel, provider) { }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="requiresNew"></param>
        /// <param name="provider"></param>
        public TransactionManager(bool requiresNew, ITransactionManagerProvider provider) : this(requiresNew, null, provider) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requiresNew"></param>
        /// <param name="isolationLevel"></param>
        /// <param name="provider"></param>
        public TransactionManager(bool requiresNew, IsolationLevel isolationLevel, ITransactionManagerProvider provider) : this(requiresNew, (IsolationLevel?)isolationLevel, provider) { }

        TransactionManager(bool requiresNew, IsolationLevel? isolationLevel, ITransactionManagerProvider provider)
        {
            this.isolationLevel = isolationLevel;

            if (!requiresNew)
            {
                parentManager = provider.GetTransactionManager();
                if (parentManager != null)
                {
                    // 继承上级
                    id = parentManager.id;
                    transactions = parentManager.transactions;
                    isolationLevel = parentManager.isolationLevel;
                    // 同步事务回调
                    parentManager.Completed += (s, e) => Completed?.Invoke(this, e);
                }
            }
        }

        #endregion

        #region OverrideMethod
        /// <summary>
        /// 返回ID的Hash值
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
        /// <summary>
        /// ID一致比较
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is TransactionManager && Equals(id, ((TransactionManager)obj).id);
        }

        #endregion

        #region Transaction Methods
        /// <summary>
        /// 为给定连接加入事务
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public DbTransaction EnlistTransaction(DbConnection connection, string connectionString)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var transaction = !isolationLevel.HasValue ? connection.BeginTransaction()
                                : connection.BeginTransaction(isolationLevel.Value);
            transactions.Add(connectionString, transaction);

            return transaction;
        }

        /// <summary>
        /// 提交
        /// </summary>
        public void Commit()
        {
            if (parentManager != null) return; // 嵌套子管理器，空提交
            var keys = transactions.Keys.ToArray();
            foreach (var key in keys)
            {
                using (var transaction = transactions[key])
                {
                    transactions.Remove(key);
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// 获取当前连接
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public bool TryGetTransaction(string connectionString, out DbTransaction transaction)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));

            TransactionManager transactionManager = this;

            while (!transactionManager.transactions.TryGetValue(connectionString, out transaction)
                && transactionManager.parentManager != null)
            {
                transactionManager = transactionManager.parentManager;
            }
            return transaction != null;
        }
        #endregion

        #region 
        void IDisposable.Dispose()
        {
            if (parentManager == null)
            {
                try
                {
                    var keys = transactions.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        using (var transaction = transactions[key])
                        {
                            transactions.Remove(key);
                            transaction.Rollback();
                        }
                    }
                }
                finally
                {
                    Completed?.Invoke(this, EventArgs.Empty);
                }

            }
        }
        #endregion
    }
}
