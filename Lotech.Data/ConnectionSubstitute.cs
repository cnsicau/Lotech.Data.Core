using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Threading;

namespace Lotech.Data
{
    /// <summary>
    /// 连接替代品
    ///     用于关注增加是否受TransactionScope管理
    /// </summary>
    public sealed class ConnectionSubstitute : IDisposable, ISite
    {
        private readonly IContainer container = new Container();
        private readonly IComponent component = new Component();
        private readonly ConnectionState initState;
        private DbConnection connection;
        private int refs = 1;

        /// <summary>
        /// 
        /// </summary>
        public ConnectionSubstitute(DbConnection connection)
        {
            if(connection  == null) throw new ArgumentNullException("connection");
            connection.Site = this;
            this.connection  = connection ;

            initState = connection.State;
        }

        /// <summary>
        /// 
        /// </summary>
        public DbConnection Connection { get { return connection; } }

        IComponent ISite.Component { get { return component; } }

        IContainer ISite.Container { get { return container; } }

        bool ISite.DesignMode { get { return false; } }

        string ISite.Name { get; set; }

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
                    using (connection)
                    {
                        connection.Close();
                    }
                    connection.Site = null;
                    connection = null;
                }
                container.Dispose();
                component.Dispose();
            }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
