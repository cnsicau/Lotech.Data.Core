using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Lotech.Data
{
    /// <summary>
    /// Transaction Manager
    /// </summary>
    public class TransactionManager : IDisposable
    {
        class Chain
        {
            [ThreadStatic]
            static Chain current;

            public static Chain Current { get { return current; } }

            Chain(Transaction transaction)
            {
                Transaction = transaction;
                Next = current;
            }

            public Transaction Transaction { get; set; }

            public Chain Next { get; }

            public static void Join(Transaction transaction)
            {
                current = new Chain(transaction);
            }

            public static void Broke()
            {
                current = current?.Next;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public class Transaction
        {
            KeyValuePair<string, DbTransaction>[] transactions;
            IsolationLevel? level;
            /// <summary>
            /// 
            /// </summary>
            public bool IsCompleted { get; private set; }

            internal Transaction(IsolationLevel? level) { this.level = level; }

            void CheckCompleted() { if (IsCompleted) throw new InvalidOperationException("transation is already completed."); }

            /// <summary>
            /// 
            /// </summary>
            public void Commit()
            {
                CheckCompleted();
                if (transactions != null)
                {
                    for (int i = 0; i < transactions.Length; i++)
                    {
                        transactions[i].Value.Commit();
                    }
                    transactions = null;
                }
                Complete();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Rollback()
            {
                CheckCompleted();
                if (transactions != null)
                {
                    for (int i = 0; i < transactions.Length; i++)
                    {
                        transactions[i].Value.Rollback();
                    }
                    transactions = null;
                }
                Complete();
            }

            #region Event
            /// <summary>
            /// 完成
            /// </summary>
            public event EventHandler Completed;
            #endregion

            void Complete()
            {
                IsCompleted = true;
                Completed?.Invoke(this, EventArgs.Empty);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="connection"></param>
            /// <param name="connectionString"></param>
            public DbTransaction Enlist(DbConnection connection, string connectionString)
            {
                CheckCompleted();

                var transaction = level.HasValue ? connection.BeginTransaction(level.Value) : connection.BeginTransaction();
                var size = transactions == null ? 0 : transactions.Length;
                Array.Resize(ref transactions, size + 1);
                transactions[size] = new KeyValuePair<string, DbTransaction>(connectionString, transaction);

                return transaction;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="connectionString"></param>
            /// <returns></returns>
            public DbTransaction GetTransaction(string connectionString)
            {
                CheckCompleted();
                if (transactions == null) return null;
                for (int i = 0; i < transactions.Length; i++)
                {
                    var transaction = transactions[i];
                    if (transaction.Key == connectionString) return transaction.Value;
                }
                return null;
            }
        }

        bool disposed;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requiresNew"></param>
        /// <param name="level"></param>
        /// <param name="suppress"></param>
        public TransactionManager(bool requiresNew = false, IsolationLevel? level = null, bool suppress = false)
        {
            if (suppress) Chain.Join(null);
            else if (Current == null || requiresNew) Chain.Join(new Transaction(level));
            else Chain.Join(Current);
        }

        /// <summary>
        /// 
        /// </summary>
        static public Transaction Current { get { return Chain.Current?.Transaction; } }

        /// <summary>
        /// 
        /// </summary>
        public void Commit()
        {
            if (Current != null)
            {
                // has continus reference
                if (Current == Chain.Current?.Next?.Transaction)
                {
                    Chain.Current.Transaction = null;
                    return;
                }
                Current?.Commit();
            }
            else
            {
                throw new InvalidOperationException("current transaction is missing.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                if (Current != null && !Current.IsCompleted)
                    Current.Rollback();

                Chain.Broke();
            }
        }
    }
}