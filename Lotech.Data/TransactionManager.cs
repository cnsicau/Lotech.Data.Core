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
        #region Types
        class TransactionManagerChain
        {
            internal readonly TransactionManager TransactionManager;
            internal readonly TransactionManagerChain Next;

            internal TransactionManagerChain(TransactionManager transactionManager, TransactionManagerChain next)
            {
                TransactionManager = transactionManager;
                Next = next;
            }
        }
        #endregion

        #region Fields

        [ThreadStatic]
        private static TransactionManagerChain currentTansactionManager;

        [ThreadStatic]
        private static int sequence;

        private readonly int id;
        private Dictionary<string, DbTransaction> transactions;
        private TransactionManager parentManager;
        private readonly IsolationLevel? isolationLevel;
        private bool disposed;
        #endregion

        #region Event
        /// <summary>
        /// 完成
        /// </summary>
        public event EventHandler Completed;
        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        public TransactionManager() : this(false, null) { }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isolationLevel"></param>
        public TransactionManager(IsolationLevel isolationLevel) : this(false, (IsolationLevel?)isolationLevel) { }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="requireNew"></param>
        public TransactionManager(bool requireNew) : this(requireNew, null) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requireNew"></param>
        /// <param name="isolationLevel"></param>
        public TransactionManager(bool requireNew, IsolationLevel isolationLevel) : this(requireNew, (IsolationLevel?)isolationLevel) { }

        TransactionManager(bool requireNew, IsolationLevel? isolationLevel)
        {
            if (currentTansactionManager != null && !requireNew)
            {
                parentManager = currentTansactionManager.TransactionManager;
                id = parentManager.id;
                transactions = parentManager.transactions;
                isolationLevel = parentManager.isolationLevel;
                parentManager.Completed += (s, e) => Completed?.Invoke(this, e);
            }
            else
            {
                id = ++sequence;
                this.isolationLevel = isolationLevel;
                transactions = new Dictionary<string, DbTransaction>();
            }
            currentTansactionManager = new TransactionManagerChain(this, currentTansactionManager);
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

            if (transactions == null) throw new InvalidOperationException("current transaction is committed or rollback.");

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
            if (parentManager != null)
            {
                transactions = null;
                return;                 // 嵌套子管理器，空提交不再同步事务
            }

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
        #endregion

        #region Static Method
        /// <summary>
        /// 获取当前事务管理器
        /// </summary>
        public static TransactionManager Current { get { return currentTansactionManager?.TransactionManager; } }

        /// <summary>
        /// 获取当前连接
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        static public bool TryGetTransaction(string connectionString, out DbTransaction transaction)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));

            if (currentTansactionManager != null)
            {
                if (currentTansactionManager.TransactionManager.transactions == null)
                    throw new InvalidOperationException("current transaction is committed or rollback.");

                if (currentTansactionManager.TransactionManager.transactions.TryGetValue(connectionString, out transaction))
                    return true;
            }

            transaction = null;
            return false;
        }
        #endregion

        #region 
        void DisposeTransactions()
        {
            if (transactions != null)
            {
                if (parentManager != null)
                {
                    parentManager.DisposeTransactions();  // 释放上层事务
                    parentManager = null;
                }
                else
                {
                    foreach (var transaction in transactions.Values)
                    {
                        using (transaction)
                        {
                            if (transaction.Connection.State == ConnectionState.Open)
                                transaction.Rollback();
                        }
                    }
                    Completed?.Invoke(this, EventArgs.Empty);
                }
                transactions = null;
            }
        }
        void IDisposable.Dispose()
        {
            if (!disposed)
            {
                DisposeTransactions();

                currentTansactionManager = currentTansactionManager.Next;
                disposed = true;
            }
        }
        #endregion
    }
}
