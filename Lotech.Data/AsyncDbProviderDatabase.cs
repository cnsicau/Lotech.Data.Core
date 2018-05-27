using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lotech.Data
{
    public partial class DbProviderDatabase : IDatabase
    {
        async Task BindOpenedConnectionAsync(DbCommand command, ConnectionSubstitute connection, CancellationToken cancellationToken)
        {
            command.Connection = connection.Connection;

            if (connection.Connection.State == ConnectionState.Open)
                return;
            if (Log == null)
            {
                await connection.Connection.OpenAsync(cancellationToken);
            }
            else
            {
                var sw = Stopwatch.StartNew();
                await connection.Connection.OpenAsync(cancellationToken);
                Log($"open connection at {DateTime.Now}. Elpased times: {sw.Elapsed}.");
                sw.Restart();
                connection.Disposed += (s, e) => Log($"close connection at {DateTime.Now}. Used times: {sw.Elapsed}");
            }
        }

        async Task<TResult> ExecuteCommandAsync<TValue, TResult>(string action, DbCommand command, Func<DbCommand, Task<TValue>> value, Func<ConnectionSubstitute, TValue, TResult> result, CancellationToken cancellationToken)
        {
            var substitute = await GetConnectionAsync(command, cancellationToken);
            try
            {
                await BindOpenedConnectionAsync(command, substitute, cancellationToken);
                TValue val;
                if (Log == null)
                    val = await value(command);
                else
                {
                    var sw = Stopwatch.StartNew();
                    LogCommand(action, command);
                    val = await value(command);
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<DataSet> ExecuteDataSetAsync(DbCommand command, CancellationToken cancellationToken)
        {
            using (var reader = await ExecuteReaderAsync(command, cancellationToken))
            {
                var dataSet = new DataSet(command.CommandText);
                var table = dataSet.Tables.Add("Result");
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                }
                var rows = new object[reader.FieldCount];
                while (await reader.ReadAsync(cancellationToken))
                {
                    reader.GetValues(rows);
                    table.Rows.Add(rows);
                }

                return dataSet;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<int> ExecuteNonQueryAsync(DbCommand command, CancellationToken cancellationToken)
        {
            return ExecuteCommandAsync(nameof(ExecuteNonQueryAsync), command, _ => _.ExecuteNonQueryAsync(cancellationToken), (connection, _) => _, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<DbDataReader> ExecuteReaderAsync(DbCommand command, CancellationToken cancellationToken)
        {
            return await ExecuteCommandAsync(nameof(ExecuteReaderAsync), command, _ => _.ExecuteReaderAsync(cancellationToken), (connection, reader) => new CompositedDataReader(reader, connection), cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<object> ExecuteScalarAsync(DbCommand command, CancellationToken cancellationToken)
        {
            return ExecuteCommandAsync(nameof(ExecuteScalarAsync), command, _ => _.ExecuteScalarAsync(cancellationToken), (connection, _) => _, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TScalar"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public Task<TScalar> ExecuteScalarAsync<TScalar>(DbCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ConnectionSubstitute> GetConnectionAsync(DbCommand command, CancellationToken cancellationToken)
        {
            if (command?.Connection?.Site is ConnectionSubstitute)
            {
                return ((ConnectionSubstitute)command.Connection.Site).Ref();
            }

            var connection = TransactionScopeConnections.GetConnection(this);
            if (connection != null)
                return connection;

            var transactionManager = TransactionManagerProvider.GetTransactionManager();
            DbTransaction transaction;
            if (transactionManager != null
                && transactionManager.TryGetTransaction(ConnectionString, out transaction))
            {
                // 绑定事务
                command.Transaction = transaction;
                command.Connection = transaction.Connection;
                return new ConnectionSubstitute(transaction.Connection).Ref();
            }

            connection = new ConnectionSubstitute(CreateConnection());

            if (transactionManager != null) // 新连接若已经存在当前事务管理器，则自动开启事务
            {
                try
                {
                    await BindOpenedConnectionAsync(command, connection, cancellationToken);
                }
                catch
                {
                    connection.Dispose();
                    throw;
                }
                transactionManager.Completed += (s, e) => connection.Dispose();
                command.Transaction = transactionManager.EnlistTransaction(connection.Connection, ConnectionString);  // 绑定事务到 DbCommand中
                connection.Ref();   // 以便上面完成时关闭连接，避免过早关闭
            }
            return connection;
        }
    }
}
