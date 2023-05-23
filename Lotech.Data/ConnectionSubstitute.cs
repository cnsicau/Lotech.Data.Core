using System;
using System.Data;
using System.Data.Common;

namespace Lotech.Data
{
    /// <summary>
    /// 连接替代品
    ///     用于关注增加是否受TransactionScope管理
    /// </summary>
    public sealed class ConnectionSubstitute : IDisposable
    {
        private readonly bool closed;
        private readonly DbConnection connection;
        private long refs = 1;

        /// <summary>
        /// 
        /// </summary>
        public ConnectionSubstitute(DbConnection connection)
        {
            this.connection = connection ?? throw new ArgumentNullException("connection");
            closed = connection.State == ConnectionState.Closed;
        }

        /// <summary>
        /// 
        /// </summary>
        public DbConnection Connection { get { return connection; } }

        /// <summary>
        /// 当真正释放时触发
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// 使用
        /// </summary>
        /// <returns></returns>
        public ConnectionSubstitute Ref()
        {
            refs++;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (--refs == 0)
            {
                if (closed) connection.Close();

                Disposed?.Invoke(this, EventArgs.Empty);
                Disposed = null;
            }
        }
    }
}
