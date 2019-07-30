using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Transactions;

namespace Lotech.Data
{

    using TransactionConnectionDictionary = ConcurrentDictionary<Transaction, List<KeyValuePair<string, ConnectionSubstitute>>>;

    /// <summary>
    /// 事务范围连接缓存
    /// </summary>
    static public class TransactionScopeConnections
    {
        /// <summary>
        /// 连接缓存
        /// </summary>
        private static readonly TransactionConnectionDictionary transactionConnections = new TransactionConnectionDictionary();

        static TransactionCompletedEventHandler OnTransactionCompleted = (s, e) =>
        {
            e.Transaction.TransactionCompleted -= OnTransactionCompleted;
            List<KeyValuePair<string, ConnectionSubstitute>> connections;
            if (transactionConnections.TryRemove(e.Transaction, out connections))
            {
                for (int i = connections.Count - 1; i >= 0; i--)
                    connections[i].Value.Dispose();
                connections.Clear();
            }
        };

        /// <summary>
        /// 获取事务范围连接
        /// </summary>
        /// <returns></returns>
        static List<KeyValuePair<string, ConnectionSubstitute>> GetTransactionConnections()
        {
            return transactionConnections.GetOrAdd(Transaction.Current, transaction =>
            {
                transaction.TransactionCompleted += OnTransactionCompleted;
                return new List<KeyValuePair<string, ConnectionSubstitute>>(1);
            });
        }

        /// <summary>
        /// 获取事务范围内的数据库连接
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000")]
        public static ConnectionSubstitute GetConnection(IDatabase database)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }
            if (Transaction.Current == null)
            {
                return null;
            }

            var transactionConnections = GetTransactionConnections();

            for (int i = 0; i < transactionConnections.Count; i++)
            {
                if (transactionConnections[i].Key == database.ConnectionString)
                    return transactionConnections[i].Value.Ref();
            }

            var connection = database.CreateConnection();
            try
            {
                connection.Open();
                var connectionSubstitute = new ConnectionSubstitute(connection);
                transactionConnections.Add(new KeyValuePair<string, ConnectionSubstitute>(database.ConnectionString, connectionSubstitute));
                return connectionSubstitute.Ref();
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }
    }
}
