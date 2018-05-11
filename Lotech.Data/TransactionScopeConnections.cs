using System;
using System.Collections.Concurrent;
using System.Transactions;

namespace Lotech.Data
{

    using TransactionConnectionDictionary = ConcurrentDictionary<Transaction, ConcurrentDictionary<string, ConnectionSubstitute>>;

    /// <summary>
    /// 事务范围连接缓存
    /// </summary>
    static public class TransactionScopeConnections
    {
        /// <summary>
        /// 连接缓存
        /// </summary>
        private static readonly TransactionConnectionDictionary transactionConnections = new TransactionConnectionDictionary();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static ConcurrentDictionary<string, ConnectionSubstitute> GetTransactionConnections()
        {
            return transactionConnections.GetOrAdd(Transaction.Current, transaction =>
            {
                transaction.TransactionCompleted += (s, e) =>
                {
                    if (transactionConnections.TryRemove(e.Transaction, out ConcurrentDictionary<string, ConnectionSubstitute> el))
                    {
                        foreach (var subsitute in el.Values)
                        {
                            subsitute.Dispose();
                        }
                    }
                };

                return new ConcurrentDictionary<string, ConnectionSubstitute>(StringComparer.CurrentCultureIgnoreCase);
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

            return transactionConnections.GetOrAdd(database.ConnectionString, (connectionString, db) =>
            {
                var connection = db.CreateConnection();
                connection.ConnectionString = connectionString;
                connection.Open();
                return new ConnectionSubstitute(connection);
            }, database).Ref();
        }
    }
}
