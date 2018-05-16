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

        [ThreadStatic]
        static Stack<TransactionManager> transactionManagers;

        private readonly Guid id = Guid.NewGuid();
        private readonly Dictionary<string, DbTransaction> transactions = new Dictionary<string, DbTransaction>();
        private readonly TransactionManager parentManager;
        private readonly IsolationLevel? isolationLevel;
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
            this.isolationLevel = isolationLevel;
            if (transactionManagers == null || transactionManagers.Count == 0)
            {
                transactionManagers = new Stack<TransactionManager>(new[] { this });
            }
            else if (requireNew)
            {
                transactionManagers.Push(this);
            }
            else // 存在父管理器时向上使用
            {
                parentManager = transactionManagers.Peek();
                // 继承上级
                id = parentManager.id;
                transactions = parentManager.transactions;
                isolationLevel = parentManager.isolationLevel;
                // 同步事务回调
                parentManager.Completed += (s, e) => Completed?.Invoke(this, e);
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
        #endregion

        #region Static Method
        /// <summary>
        /// 获取当前事务管理器
        /// </summary>
        public static TransactionManager Current
        {
            get
            {
                return transactionManagers?.Count > 0 ? transactionManagers.Peek() : null;
            }
        }

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

            if (transactionManagers != null && transactionManagers.Count > 0)
            {
                foreach (var tm in transactionManagers)
                {
                    if (tm.transactions.TryGetValue(connectionString, out transaction)) return true;
                }
            }

            transaction = null;
            return false;
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
                    transactionManagers.Pop();
                    if (transactionManagers.Count == 0)
                        transactionManagers = null;
                }

                Completed?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion
    }
}
