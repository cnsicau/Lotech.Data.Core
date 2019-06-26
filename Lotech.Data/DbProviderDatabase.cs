using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace Lotech.Data
{
    /// <summary>
    /// DbProvider 实现
    /// </summary>
    public abstract class DbProviderDatabase : DbDatabase, IDatabase
    {
        private readonly DbProviderFactory dbProviderFactory;

        /// <summary>
        /// 获取 DbProviderFactory 实例
        /// </summary>
        public DbProviderFactory DbProviderFactory { get { return dbProviderFactory; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbProviderFactory"></param>
        /// <param name="services"></param>
        protected DbProviderDatabase(DbProviderFactory dbProviderFactory, IEntityServices services)
            : base(services)
        {
            if (dbProviderFactory == null)
                throw new ArgumentNullException(nameof(dbProviderFactory));

            this.dbProviderFactory = dbProviderFactory;
        }

        void BindOpenedConnection(DbCommand command, ConnectionSubstitute connection)
        {
            command.Connection = connection.Connection;

            if (connection.Connection.State == ConnectionState.Open)
                return;
            if (Log == null)
            {
                connection.Connection.Open();
            }
            else
            {
                var sw = Stopwatch.StartNew();
                connection.Connection.Open();
                Log($"open connection at {DateTime.Now}. Elpased times: {sw.Elapsed}.");
                sw.Restart();
                connection.Disposed += (s, e) => Log($"close connection at {DateTime.Now}. Used times: {sw.Elapsed}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal virtual ConnectionSubstitute GetConnection(DbCommand command)
        {
            ConnectionSubstitute connection;
            if (TransactionManager.Current != null)
            {
                DbTransaction transaction;
                if (TransactionManager.TryGetTransaction(ConnectionString, out transaction))
                {
                    // 绑定事务
                    command.Transaction = transaction;
                    command.Connection = transaction.Connection;
                    return new ConnectionSubstitute(transaction.Connection).Ref();
                }
            }
            else
            {
                connection = TransactionScopeConnections.GetConnection(this);
                if (connection != null) return connection.Ref();
            }

            connection = new ConnectionSubstitute(CreateConnection());

            if (TransactionManager.Current != null) // 新连接若已经存在当前事务管理器，则自动开启事务
            {
                try
                { BindOpenedConnection(command, connection); }
                catch
                {
                    connection.Dispose();
                    throw;
                }
                TransactionManager.Current.Completed += (s, e) => connection.Dispose();
                command.Transaction = TransactionManager.Current.EnlistTransaction(connection.Connection, ConnectionString);  // 绑定事务到 DbCommand中
                return connection.Ref();   // 以便上面完成时关闭连接，避免过早关闭
            }
            return connection;
        }

        TResult ExecuteCommand<TValue, TResult>(string action, DbCommand command, Func<DbCommand, TValue> value, Func<ConnectionSubstitute, TValue, TResult> result)
        {
            var substitute = GetConnection(command);
            try
            {
                BindOpenedConnection(command, substitute);
                TValue val;
                if (Log == null)
                    val = value(command);
                else
                {
                    LogCommand(action, command);
                    var sw = Stopwatch.StartNew();
                    val = value(command);
                    Log("  -- elapsed times: " + sw.Elapsed);
                }
                return result(substitute, val);
            }
            catch
            {
                substitute.Dispose();
                throw;
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
            var substitute = GetConnection(command);
            try
            {
                BindOpenedConnection(command, substitute);
                IDataReader reader;
                if (Log == null)
                    reader = command.ExecuteReader(behavior);
                else
                {
                    LogCommand("ExecuteReader", command);
                    var sw = Stopwatch.StartNew();
                    reader = command.ExecuteReader(behavior);
                    Log("  -- elapsed times: " + sw.Elapsed);
                }
                return new CompositedDataReader(reader, substitute);
            }
            catch
            {
                substitute.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override int ExecuteNonQuery(DbCommand command)
        {
            return ExecuteCommand(nameof(ExecuteNonQuery), command, _ => _.ExecuteNonQuery(), (substitute, val) => { using (substitute) { return val; } });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override DataSet ExecuteDataSet(DbCommand command)
        {
            return ExecuteCommand(nameof(ExecuteDataSet), command, _ => _.ExecuteReader(),
                (connection, reader) =>
                {
                    using (connection)
                    {
                        using (reader)
                        {
                            var dataSet = new DataSet(command.CommandText);
                            var index = 0;
                            do
                            {
                                var table = dataSet.Tables.Add("Table" + (index++ == 0 ? "" : index.ToString()));
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                                }
                                var rows = new object[reader.FieldCount];
                                while (reader.Read())
                                {
                                    reader.GetValues(rows);
                                    table.Rows.Add(rows);
                                }
                            } while (reader.NextResult());

                            return dataSet;
                        }
                    }
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override object ExecuteScalar(DbCommand command)
        {
            return ExecuteCommand(nameof(ExecuteScalar), command, _ => _.ExecuteScalar(), (substitute, val) =>
            {
                substitute.Dispose();
                return val == DBNull.Value ? null : val;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DbConnection CreateConnection()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new InvalidProgramException("ConnectionString is empty");
            }

            var connection = dbProviderFactory.CreateConnection();
            connection.ConnectionString = ConnectionString;
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
            var command = dbProviderFactory.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            return command;
        }
    }
}