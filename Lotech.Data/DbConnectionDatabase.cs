using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace Lotech.Data
{
    /// <summary>
    /// 基于连接 实现
    /// </summary>
    public abstract class DbConnectionDatabase : DbDatabase, IDatabase
    {
        private readonly DbConnection connection;

        /// <summary>
        /// 获取 连接 实例
        /// </summary>
        public DbConnection Connection { get { return connection; } }

        /// <summary>
        /// 
        /// </summary>
        public override string ConnectionString
        {
            get { return connection.ConnectionString; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="services"></param>
        protected DbConnectionDatabase(DbConnection connection, IEntityServices services)
            : base(services)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            this.connection = connection;
        }

        /// <summary>
        /// 返回当前包裹的连接
        /// </summary>
        /// <returns></returns>
        public override DbConnection CreateConnection()
        {
            return connection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public override DbCommand GetCommand(CommandType commandType, string commandText)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            return command;
        }

        void EnlistTransaction(DbCommand command)
        {
            if (TransactionManager.Current != null)
            {
                DbTransaction transaction;
                if (!TransactionManager.TryGetTransaction(ConnectionString, out transaction))
                {
                    transaction = TransactionManager.Current.EnlistTransaction(connection, ConnectionString);
                }
                command.Transaction = transaction;
            }
        }

        TResult ExecuteCommand<TResult>(string action, DbCommand command, CommandBehavior behavior, Func<DbCommand, CommandBehavior, TResult> execute)
        {
            var closed = connection.State == ConnectionState.Closed;
            try
            {
                if (command.Connection != connection)
                    command.Connection = connection;

                if (Log == null)
                {
                    if (closed) connection.Open();

                    EnlistTransaction(command);
                    return execute(command, behavior);
                }
                else
                {
                    LogCommand(action, command);
                    var sw = Stopwatch.StartNew();
                    if (closed)
                    {
                        connection.Open();
                        EnlistTransaction(command);
                        Log($"open connection at {DateTime.Now}. Elpased times: {sw.Elapsed}.");
                        sw.Restart();
                        connection.Disposed += (s, e) => Log($"close connection at {DateTime.Now}. Used times: {sw.Elapsed}");
                    }

                    var val = execute(command, behavior);
                    Log("  -- elapsed times: " + sw.Elapsed);
                    return val;
                }
            }
            catch
            {
                if (closed && connection.State == ConnectionState.Open) connection.Close();
                throw;
            }
            finally
            {
                if (!typeof(IDataReader).IsAssignableFrom(typeof(TResult))
                    && closed && connection.State == ConnectionState.Open) connection.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public override IDataReader ExecuteReader(DbCommand command, CommandBehavior behavior)
        {
            return ExecuteCommand<IDataReader>(nameof(ExecuteReader), command, behavior
                , (c, b) => c.ExecuteReader(b));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override object ExecuteScalar(DbCommand command)
        {
            return ExecuteCommand(nameof(ExecuteScalar), command, CommandBehavior.Default
                , (c, b) => c.ExecuteScalar());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override int ExecuteNonQuery(DbCommand command)
        {
            return ExecuteCommand(nameof(ExecuteReader), command, CommandBehavior.Default
                , (c, b) => c.ExecuteNonQuery());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override DataSet ExecuteDataSet(DbCommand command)
        {
            return ExecuteCommand(nameof(ExecuteDataSet), command, CommandBehavior.SequentialAccess,
                (dbCommand, behavior) =>
                {
                    using (var reader = dbCommand.ExecuteReader(behavior))
                    {
                        return CreateDataSet(reader);
                    }
                });
        }
    }
}