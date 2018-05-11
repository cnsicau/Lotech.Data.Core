using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Diagnostics;
using Lotech.Data.Queries;

namespace Lotech.Data
{
    /// <summary>
    /// DbProvider 实现
    /// </summary>
    public abstract class DbProviderDatabase : IDatabase
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly DbProviderFactory dbProviderFactory;
        private readonly IEntityServices services;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbProviderFactory"></param>
        /// <param name="services"></param>
        protected DbProviderDatabase(DbProviderFactory dbProviderFactory, IEntityServices services)
        {
            this.dbProviderFactory = dbProviderFactory ?? throw new ArgumentNullException(nameof(dbProviderFactory)); ;
            this.services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual Action<string> Log { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string ConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual DbConnection CreateConnection()
        {
            if (String.IsNullOrEmpty(ConnectionString))
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
        /// <returns></returns>
        protected virtual ConnectionSubstitute GetConnection(DbCommand command)
        {
            if (command.Connection != null)
            {
                if (command.Connection.State == System.Data.ConnectionState.Open)
                {
                    return new ConnectionSubstitute(command.Connection).Ref();
                }
                return new ConnectionSubstitute(command.Connection);
            }

            var connection = TransactionScopeConnections.GetConnection(this);
            if (connection != null) return connection;

            var transactionManager = TransactionManager.Current;
            if (transactionManager != null
                && TransactionManager.TryGetTransaction(ConnectionString, out DbTransaction transaction))
            {
                // 绑定事务
                command.Transaction = transaction;
                return new ConnectionSubstitute(transaction.Connection).Ref();
            }

            connection = new ConnectionSubstitute(CreateConnection());

            if (transactionManager != null) // 新连接若已经存在当前事务管理器，则自动开启事务
            {
                connection.Connection.Open();
                transactionManager.Completed += (s, e) => connection.Dispose();
                command.Transaction = transactionManager.EnlistTransaction(connection.Connection, ConnectionString);  // 绑定事务到 DbCommand中
                connection.Ref();   // 以便上面完成时关闭连接，避免过早关闭
            }
            return connection;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100")]
        public virtual DbCommand GetCommand(System.Data.CommandType commandType, string commandText)
        {
            var command = dbProviderFactory.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            return command;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual DbCommand GetSqlStringCommand(string query)
        {
            return GetCommand(System.Data.CommandType.Text, query);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public virtual DbCommand GetStoredProcedureCommand(string procedureName)
        {
            return GetCommand(System.Data.CommandType.StoredProcedure, procedureName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="direction"></param>
        /// <param name="value"></param>
        public virtual void AddParameter(DbCommand command, string parameterName, System.Data.DbType dbType, System.Data.ParameterDirection direction, object value)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentNullException("parameterName");
            }

            var parameter = dbProviderFactory.CreateParameter();
            parameter.Direction = direction;
            parameter.DbType = dbType;
            parameter.Value = value ?? DBNull.Value;
            parameter.ParameterName = parameterName;

            command.Parameters.Add(parameter);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="direction"></param>
        /// <param name="size"></param>
        /// <param name="nullable"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="value"></param>
        public virtual void AddParameter(DbCommand command, string parameterName, System.Data.DbType dbType, System.Data.ParameterDirection direction, int size, bool nullable, int precision, int scale, object value)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentNullException("parameterName");
            }

            var parameter = dbProviderFactory.CreateParameter();
            parameter.Direction = System.Data.ParameterDirection.Output;
            parameter.DbType = dbType;
            parameter.SourceColumnNullMapping = nullable;
            parameter.Size = size;
            parameter.ParameterName = parameterName;

            command.Parameters.Add(parameter);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        public virtual void AddInParameter(DbCommand command, string parameterName, System.Data.DbType dbType, object value)
        {
            AddParameter(command, parameterName, dbType, System.Data.ParameterDirection.Input, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        public virtual void AddOutParameter(DbCommand command, string parameterName, System.Data.DbType dbType, int size)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentNullException("parameterName");
            }

            var parameter = dbProviderFactory.CreateParameter();
            parameter.Direction = System.Data.ParameterDirection.Output;
            parameter.DbType = dbType;
            parameter.Size = size;
            parameter.ParameterName = parameterName;

            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract string BuildParameterName(string name);

        /// <summary>
        ///  获取参数名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract string QuoteName(string name);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual System.Data.IDataReader ExecuteReader(DbCommand command)
        {
            var substitute = GetConnection(command);
            try
            {
                command.Connection = substitute.Connection;

                if (substitute.Connection.State != System.Data.ConnectionState.Open)
                    substitute.Connection.Open();

                Debug.WriteLine("Execute query:" + command.CommandText);
                return new CompositedDataReader(command.ExecuteReader(), substitute);
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
        /// <param name="commandText"></param>
        /// <returns></returns>
        public System.Data.IDataReader ExecuteReader(string commandText)
        {
            return ExecuteReader(System.Data.CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual System.Data.IDataReader ExecuteReader(System.Data.CommandType commandType, string commandText)
        {
            var command = GetCommand(commandType, commandText);
            try
            {
                return ExecuteReader(command);
            }
            catch
            {
                if (command != null)
                    command.Dispose();
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual object ExecuteScalar(DbCommand command)
        {
            using (var substitute = GetConnection(command))
            {
                Debug.WriteLine("Execute query:" + command.CommandText);
                command.Connection = substitute.Connection;
                if (substitute.Connection.State != System.Data.ConnectionState.Open)
                    substitute.Connection.Open();

                object ret = command.ExecuteScalar();
                return ret == DBNull.Value ? null : ret;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public object ExecuteScalar(string commandText)
        {
            return ExecuteScalar(System.Data.CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual object ExecuteScalar(System.Data.CommandType commandType, string commandText)
        {
            using (var command = GetCommand(commandType, commandText))
            {
                return this.ExecuteScalar(command);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TScalar"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual TScalar ExecuteScalar<TScalar>(DbCommand command)
        {
            using (var subsitute = GetConnection(command))
            {
                command.Connection = subsitute.Connection;
                return new CommandQueryResult<TScalar>(command
                        , new SimpleResultMapper<TScalar>(), Log).FirstOrDefault();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TScalar"></typeparam>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>(string commandText)
        {
            return ExecuteScalar<TScalar>(System.Data.CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TScalar"></typeparam>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual TScalar ExecuteScalar<TScalar>(System.Data.CommandType commandType, string commandText)
        {
            using (var command = GetCommand(commandType, commandText))
            {
                return this.ExecuteScalar<TScalar>(command);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000")]
        public virtual System.Data.DataSet ExecuteDataSet(DbCommand command)
        {
            using (DbDataAdapter adapter = dbProviderFactory.CreateDataAdapter())
            {
                Debug.WriteLine("Execute query:" + command.CommandText);

                adapter.SelectCommand = command;
                using (var subsitute = GetConnection(command))
                {
                    command.Connection = subsitute.Connection;
                    if (subsitute.Connection.State != System.Data.ConnectionState.Open)
                        subsitute.Connection.Open();

                    var dataSet = new System.Data.DataSet("Table");
                    adapter.Fill(dataSet);
                    return dataSet;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000")]
        public System.Data.DataSet ExecuteDataSet(string commandText)
        {
            return ExecuteDataSet(System.Data.CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000")]
        public virtual System.Data.DataSet ExecuteDataSet(System.Data.CommandType commandType, string commandText)
        {
            using (var command = GetCommand(commandType, commandText))
            {
                return this.ExecuteDataSet(command);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual int ExecuteNonQuery(DbCommand command)
        {
            using (var subsitute = GetConnection(command))
            {
                Debug.WriteLine("Execute query:" + command.CommandText);
                command.Connection = subsitute.Connection;
                if (subsitute.Connection.State != System.Data.ConnectionState.Open)
                    subsitute.Connection.Open();
                return command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(System.Data.CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual int ExecuteNonQuery(System.Data.CommandType commandType, string commandText)
        {
            using (var command = GetCommand(commandType, commandText))
            {
                return ExecuteNonQuery(command);
            }
        }

        #region Exists
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool Exists<EntityType, TKey>(TKey key) where EntityType : class
        {
            return services.ExistsByKey<EntityType, TKey>()(this, key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual bool Exists<EntityType>(EntityType entity) where EntityType : class
        {
            return services.Exists<EntityType>()(this, entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual bool Exists<EntityType>(Expression<Func<EntityType, bool>> predicate) where EntityType : class
        {
            return services.ExistsByPredicate<EntityType>()(this, predicate);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual EntityType LoadEntity<EntityType, TKey>(TKey key) where EntityType : class
        {
            return services.LoadEntityByKey<EntityType, TKey>()(this, key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual EntityType LoadEntity<EntityType>(EntityType entity) where EntityType : class
        {
            return services.LoadEntity<EntityType>()(this, entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual EntityType LoadEntity<EntityType>(Expression<Func<EntityType, bool>> predicate) where EntityType : class
        {
            return services.LoadEntityByPredicate<EntityType>()(this, predicate);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual EntityType ExecuteEntity<EntityType>(DbCommand command) where EntityType : class
        {
            using (var subsitute = GetConnection(command))
            {
                command.Connection = subsitute.Connection;
                return new CommandQueryResult<EntityType>(
                        command
                        , new EntityResultMapper<EntityType>()
                        , Log).FirstOrDefault();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual IEnumerable<EntityType> ExecuteEntities<EntityType>(DbCommand command) where EntityType : class
        {
            using (var subsitute = GetConnection(command))
            {
                command.Connection = subsitute.Connection;
                return new CommandQueryResult<EntityType>(
                    command
                    , new EntityResultMapper<EntityType>()
                    , Log).ToArray();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual EntityType ExecuteEntity<EntityType>(string commandText) where EntityType : class
        {
            return ExecuteEntity<EntityType>(System.Data.CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual EntityType ExecuteEntity<EntityType>(System.Data.CommandType commandType, string commandText)
             where EntityType : class
        {
            using (var command = dbProviderFactory.CreateCommand())
            {
                command.CommandText = commandText;
                command.CommandType = commandType;

                return ExecuteEntity<EntityType>(command);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual IEnumerable<EntityType> ExecuteEntities<EntityType>(string commandText) where EntityType : class
        {
            return ExecuteEntities<EntityType>(System.Data.CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual IEnumerable<EntityType> ExecuteEntities<EntityType>(System.Data.CommandType commandType, string commandText)
            where EntityType : class
        {
            using (var command = dbProviderFactory.CreateCommand())
            {
                command.CommandText = commandText;
                command.CommandType = commandType;

                return ExecuteEntities<EntityType>(command);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entity"></param>
        public virtual void InsertEntity<EntityType>(EntityType entity) where EntityType : class
        {
            services.InsertEntity<EntityType>()(this, entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entities"></param>
        public virtual void InsertEntities<EntityType>(IEnumerable<EntityType> entities) where EntityType : class
        {
            services.InsertEntities<EntityType>()(this, entities);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entity"></param>
        public virtual void UpdateEntity<EntityType>(EntityType entity) where EntityType : class
        {
            services.UpdateEntity<EntityType>()(this, entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entities"></param>
        public virtual void UpdateEntities<EntityType>(IEnumerable<EntityType> entities) where EntityType : class
        {
            services.UpdateEntities<EntityType>()(this, entities);
        }

        /// <summary>
        /// 仅更新包含部分
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TInclude"></typeparam>
        /// <param name="entity"></param>
        /// <param name="include">仅更新部分，如  _ => new { _.ModifyTime, _.Deleted } </param>
        public virtual void UpdateEntityInclude<EntityType, TInclude>(EntityType entity, Func<EntityType, TInclude> include) where EntityType : class where TInclude : class
        {
            services.UpdateEntityInclude<EntityType, TInclude>()(this, entity);
        }

        /// <summary>
        /// 仅更新包含部分
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TInclude"></typeparam>
        /// <param name="entities"></param>
        /// <param name="include">仅更新部分，如  _ => new { _.ModifyTime, _.Deleted } </param>
        public virtual void UpdateEntitiesInclude<EntityType, TInclude>(IEnumerable<EntityType> entities, Func<EntityType, TInclude> include) where EntityType : class where TInclude : class
        {
            services.UpdateEntitiesInclude<EntityType, TInclude>()(this, entities);
        }

        /// <summary>
        /// 更新除指定内容部分
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TExclude"></typeparam>
        /// <param name="entity"></param>
        /// <param name="exclude">排除部分，如  _ => new { _.Code, _.CreateTime } </param>
        public virtual void UpdateEntityExclude<EntityType, TExclude>(EntityType entity, Func<EntityType, TExclude> exclude) where EntityType : class where TExclude : class
        {
            services.UpdateEntityExclude<EntityType, TExclude>()(this, entity);
        }

        /// <summary>
        /// 更新除指定内容部分
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TExclude"></typeparam>
        /// <param name="entities"></param>
        /// <param name="exclude">排除部分，如  _ => new { _.Code, _.CreateTime } </param>
        public virtual void UpdateEntitiesExclude<EntityType, TExclude>(IEnumerable<EntityType> entities, Func<EntityType, TExclude> exclude) where EntityType : class where TExclude : class
        {
            services.UpdateEntitiesExclude<EntityType, TExclude>()(this, entities);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="id"></param>
        public virtual void DeleteEntity<EntityType, TKey>(TKey id) where EntityType : class
        {
            services.DeleteEntityByKey<EntityType, TKey>()(this, id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entity"></param>
        public virtual void DeleteEntity<EntityType>(EntityType entity) where EntityType : class
        {
            services.DeleteEntity<EntityType>()(this, entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entities"></param>
        public virtual void DeleteEntities<EntityType>(IEnumerable<EntityType> entities) where EntityType : class
        {
            services.DeleteEntities<EntityType>()(this, entities);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="predicate"></param>
        public virtual void DeleteEntities<EntityType>(Expression<Func<EntityType, bool>> predicate) where EntityType : class
        {
            services.DeleteEntitiesByPredicate<EntityType>()(this, predicate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <returns></returns>
        public virtual IEnumerable<EntityType> FindEntities<EntityType>() where EntityType : class
        {
            return services.FindEntities<EntityType>()(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual IEnumerable<EntityType> FindEntities<EntityType>(Expression<Func<EntityType, bool>> predicate) where EntityType : class
        {
            return services.FindEntitiesByPredicate<EntityType>()(this, predicate);
        }
    }
}
