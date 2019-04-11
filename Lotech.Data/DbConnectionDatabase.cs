﻿using System;
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
        /// 
        /// </summary>
        /// <returns></returns>
        public override DbConnection CreateConnection()
        {
            throw new NotSupportedException("use Connection property");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DbCommand CreateCommand()
        {
            return connection.CreateCommand();
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
                    return execute(command, behavior);
                }
                else
                {
                    LogCommand(action, command);
                    var sw = Stopwatch.StartNew();
                    if (closed)
                    {
                        connection.Open();
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
                if (closed && connection.State != ConnectionState.Closed) connection.Close();
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
            var closed = command.Connection.State != ConnectionState.Open;
            try
            {
                if (command.Connection.State != ConnectionState.Open)
                    command.Connection.Open();
                return command.ExecuteScalar();
                //return ExecuteCommand<object>(nameof(ExecuteReader), command, CommandBehavior.Default
                //    , (c, b) => c.ExecuteScalar());
            }
            finally
            {
                if (closed) command.Connection.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override int ExecuteNonQuery(DbCommand command)
        {
            return ExecuteCommand<int>(nameof(ExecuteReader), command, CommandBehavior.Default
                , (c, b) => c.ExecuteNonQuery());
        }
    }
}