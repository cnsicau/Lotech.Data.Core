using System;
using System.Data;
using System.Data.Common;
using System.Threading;

namespace Lotech.Data
{
    /// <summary>
    /// 连接替代品
    ///     用于关注增加是否受TransactionScope管理
    /// </summary>
    public sealed class ConnectionSubstitute : IDisposable
    {
        private readonly DbConnection connection;
        private readonly ConnectionState initState;
        private int refs = 1;

        /// <summary>
        /// 
        /// </summary>
        public ConnectionSubstitute(DbConnection connection)
        {
            this.connection = connection ?? throw new ArgumentNullException("connection");
            initState = connection.State;
        }

        /// <summary>
        /// 
        /// </summary>
        public DbConnection Connection { get { return connection; } }

        /// <summary>
        /// 使用
        /// </summary>
        /// <returns></returns>
        public ConnectionSubstitute Ref()
        {
            Interlocked.Increment(ref refs);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Decrement(ref refs) == 0)
            {
                if (initState == ConnectionState.Closed && connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
    }
}
