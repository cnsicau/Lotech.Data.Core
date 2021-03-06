﻿using Lotech.Data.Descriptors;
using Lotech.Data.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Lotech.Data
{
    /// <summary>
    /// DbProvider 实现
    /// </summary>
    public abstract class DbDatabase : IDatabase
    {
        static readonly bool debugging = Debugger.IsAttached;
        /// <summary>
        /// 
        /// </summary>
        private readonly IEntityServices services;
        private readonly bool trace = Configurations.DatabaseConfiguration.Current?.DatabaseSettings?.Trace ?? false;

        static void TraceLog(string message) { Trace.WriteLine(message); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="command"></param>
        protected virtual void LogCommand(string action, DbCommand command)
        {
            if (Log != null) LogCommandAction(action, command);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="command"></param>
        protected void LogCommandAction(string action, DbCommand command)
        {
            var log = new StringBuilder();
            log.AppendLine().Append(action).Append('(').Append(command.CommandType)
                .AppendLine(") :").AppendLine(command.CommandText);
            foreach (DbParameter p in command.Parameters)
            {
                log.Append("  -- ").AppendFormat("{0,-12}", p.ParameterName)
                    .AppendFormat("{0,10}", p.DbType).Append("    =  ")
                    .Append(p.Value).AppendLine();
            }
            Log(log.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        protected DbDatabase(IEntityServices services)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
            services.Database = this;
            DescriptorProvider = DefaultDescriptorProvider.Instance;

            if (trace || debugging) Log = TraceLog;
        }

        /// <summary>
        /// 由 DataReader 创建 DataSet
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected virtual DataSet CreateDataSet(IDataReader reader)
        {
            var dataSet = new DataSet();
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

        /// <summary>
        /// 
        /// </summary>
        public virtual IDescriptorProvider DescriptorProvider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual Action<string> Log { get; set; }

        /// <summary>
        /// 启用跟踪日志，便于输出执行中的SQL语句
        /// </summary>
        [Obsolete]
        public virtual void EnableTraceLog()
        {
            Log = TraceLog;
        }

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
        public abstract DbConnection CreateConnection();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual DbType ParseDbType(Type type) { return Utils.DbTypeParser.Parse(type); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public abstract DbCommand GetCommand(CommandType commandType, string commandText);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual DbCommand GetSqlStringCommand(string query)
        {
            return GetCommand(CommandType.Text, query);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public virtual DbCommand GetStoredProcedureCommand(string procedureName)
        {
            return GetCommand(CommandType.StoredProcedure, procedureName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="direction"></param>
        /// <param name="value"></param>
        public virtual void AddParameter(DbCommand command, string parameterName, DbType dbType, ParameterDirection direction, object value)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentNullException("parameterName");
            }

            var parameter = command.CreateParameter();
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
        public virtual void AddParameter(DbCommand command, string parameterName, DbType dbType, ParameterDirection direction, int size, bool nullable, int precision, int scale, object value)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentNullException("parameterName");
            }

            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Output;
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
        public virtual void AddInParameter(DbCommand command, string parameterName, DbType dbType, object value)
        {
            AddParameter(command, parameterName, dbType, ParameterDirection.Input, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        public virtual void AddOutParameter(DbCommand command, string parameterName, DbType dbType, int size)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentNullException("parameterName");
            }

            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Output;
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
        public virtual IDataReader ExecuteReader(DbCommand command)
        {
            return ExecuteReader(command, CommandBehavior.Default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public abstract IDataReader ExecuteReader(DbCommand command, CommandBehavior behavior);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string commandText)
        {
            return ExecuteReader(CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual IDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            var command = GetCommand(commandType, commandText);
            try
            {
                return new CompositedDataReader(ExecuteReader(command), command);
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
        public abstract object ExecuteScalar(DbCommand command);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public object ExecuteScalar(string commandText)
        {
            return ExecuteScalar(CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual object ExecuteScalar(CommandType commandType, string commandText)
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
            using (var reader = ExecuteReader(command, CommandBehavior.SingleRow))
            {
                var mapper = ResultMapper<TScalar>.Create(this);
                return new QueryResult<TScalar>(reader, mapper).FirstOrDefault();
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
            return ExecuteScalar<TScalar>(CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TScalar"></typeparam>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual TScalar ExecuteScalar<TScalar>(CommandType commandType, string commandText)
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
        public abstract DataSet ExecuteDataSet(DbCommand command);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000")]
        public DataSet ExecuteDataSet(string commandText)
        {
            return ExecuteDataSet(CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000")]
        public virtual DataSet ExecuteDataSet(CommandType commandType, string commandText)
        {
            using (var command = GetCommand(commandType, commandText))
            {
                return ExecuteDataSet(command);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public abstract int ExecuteNonQuery(DbCommand command);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual int ExecuteNonQuery(CommandType commandType, string commandText)
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
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual dynamic ExecuteEntity(DbCommand command)
        {
            using (var reader = ExecuteReader(command, CommandBehavior.SingleRow | CommandBehavior.SequentialAccess))
            {
                using (IEnumerator<dynamic> enumerator = new QueryResult<dynamic>(reader, new ObjectResultMapper { Database = this }))
                {
                    if (enumerator.MoveNext()) return enumerator.Current;
                }
                return default(dynamic);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual dynamic[] ExecuteEntities(DbCommand command)
        {
            var entities = new List<object>();
            using (var reader = ExecuteReader(command, CommandBehavior.SequentialAccess))
            {
                using (IEnumerator<object> enumerator = new QueryResult<dynamic>(reader, new ObjectResultMapper { Database = this }))
                {
                    while (enumerator.MoveNext()) entities.Add(enumerator.Current);
                }
            }
            return entities.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual dynamic ExecuteEntity(CommandType commandType, string commandText)
        {
            using (var command = GetCommand(commandType, commandText))
            {
                return ExecuteEntity(command);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual dynamic[] ExecuteEntities(CommandType commandType, string commandText)
        {
            using (var command = GetCommand(commandType, commandText))
            {
                return ExecuteEntities(command);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual dynamic ExecuteEntity(string commandText)
        {
            return ExecuteEntity(CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual dynamic[] ExecuteEntities(string commandText)
        {
            return ExecuteEntities(CommandType.Text, commandText);
        }

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
        public virtual EntityType ExecuteEntity<EntityType>(DbCommand command)
        {
            using (var reader = ExecuteReader(command, CommandBehavior.SingleRow | CommandBehavior.SequentialAccess))
            {
                return new QueryResult<EntityType>(reader, ResultMapper<EntityType>.Create(this)).FirstOrDefault();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual EntityType[] ExecuteEntities<EntityType>(DbCommand command)
        {
            using (var reader = ExecuteReader(command, CommandBehavior.SequentialAccess))
            {
                return new QueryResult<EntityType>(reader, ResultMapper<EntityType>.Create(this)).ToArray();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual EntityType ExecuteEntity<EntityType>(string commandText)
        {
            return ExecuteEntity<EntityType>(CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual EntityType ExecuteEntity<EntityType>(CommandType commandType, string commandText)
        {
            using (var command = GetCommand(commandType, commandText))
            {
                return ExecuteEntity<EntityType>(command);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual EntityType[] ExecuteEntities<EntityType>(string commandText)
        {
            return ExecuteEntities<EntityType>(CommandType.Text, commandText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual EntityType[] ExecuteEntities<EntityType>(CommandType commandType, string commandText)
        {
            using (var command = GetCommand(commandType, commandText))
            {
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
        public virtual EntityType[] FindEntities<EntityType>() where EntityType : class
        {
            return services.FindEntities<EntityType>()(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual EntityType[] FindEntities<EntityType>(Expression<Func<EntityType, bool>> predicate) where EntityType : class
        {
            return services.FindEntitiesByPredicate<EntityType>()(this, predicate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TSet"></typeparam>
        /// <param name="entity"></param>
        /// <param name="sets"></param>
        /// <param name="predicate"></param>
        public virtual void UpdateEntities<EntityType, TSet>(EntityType entity, Func<EntityType, TSet> sets, Expression<Func<EntityType, bool>> predicate)
            where EntityType : class
            where TSet : class
        {
            services.UpdateEntities<EntityType, TSet>()(this, entity, predicate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public virtual int Count<EntityType>(Expression<Func<EntityType, bool>> conditions) where EntityType : class
        {
            return services.CountByPredicate<EntityType>()(this, conditions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <returns></returns>
        public virtual int Count<EntityType>() where EntityType : class
        {
            return services.Count<EntityType>()(this);
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> ExecuteEntityReader<TEntity>(DbCommand command)
        {
            IDataReader reader = ExecuteReader(command, CommandBehavior.SequentialAccess);
            return new QueryResult<TEntity>(reader, ResultMapper<TEntity>.Create(this));
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> ExecuteEntityReader<TEntity>(string query)
        {
            return ExecuteEntityReader<TEntity>(CommandType.Text, query);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="commandType"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> ExecuteEntityReader<TEntity>(CommandType commandType, string text)
        {
            var command = GetCommand(commandType, text);
            IDataReader reader = null;
            try
            {
                reader = ExecuteReader(command, CommandBehavior.SequentialAccess);
                reader = new CompositedDataReader(reader, command);
                return new QueryResult<TEntity>(reader, ResultMapper<TEntity>.Create(this));
            }
            catch
            {
                reader?.Dispose();
                command.Dispose();
                throw;
            }
        }

        #region Queries

        /// <summary>
        /// 构建无初始SQL的实例
        /// </summary>
        /// <returns></returns>
        public ISqlQuery SqlQuery()
        {
            return new SqlQuery(this);
        }

        /// <summary>
        /// 构建指定容量SQL的实例
        /// </summary>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public ISqlQuery SqlQuery(int capacity)
        {
            return new SqlQuery(this, capacity);
        }

        /// <summary>
        /// 构建指定初始SQL的实例
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public ISqlQuery SqlQuery(string sql)
        {
            return new SqlQuery(this, sql);
        }

        /// <summary>
        /// 构建指定初始SQL的实例
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public ISqlQuery SqlQueryString(string sql, int offset, int length)
        {
            return new SqlQuery(this, sql, offset, length);
        }

        /// <summary>
        /// 构建指定初始SQL的实例并在末尾追加换行
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public ISqlQuery SqlQueryLine(string sql)
        {
            return new SqlQuery(this, sql).AppendLine();
        }

        /// <summary>
        /// 构建指定初始SQL的实例并在末尾追加换行
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public ISqlQuery SqlQueryStringLine(string sql, int offset, int length)
        {
            return new SqlQuery(this, sql, offset, length).AppendLine();
        }

        /// <summary>构建指定初始SQL、参数实例</summary>
        /// <param name="sql">初始SQL语句，可使用 {0}、{1}…{n}，向后引用args位置上的参数值</param>
        /// <param name="args">用于sql中的参数引用</param>
        /// <returns></returns>
        public ISqlQuery SqlQuery(string sql, params object[] args)
        {
            return new SqlQuery(this, sql.Length + args.Length * 8).Append(sql, args);
        }

        /// <summary>构建指定初始SQL、参数实例并在末尾追加换行</summary>
        /// <param name="sql">初始SQL语句，可使用 {0}、{1}…{n}，向后引用args位置上的参数值</param>
        /// <param name="args">用于sql中的参数引用</param>
        /// <returns></returns>
        public ISqlQuery SqlQueryLine(string sql, params object[] args)
        {
            return new SqlQuery(this).AppendLine(sql, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public IProcedureQuery ProcedureQuery(string procedureName)
        {
            if (procedureName == null) throw new ArgumentNullException(nameof(procedureName));

            return new ProcedureQuery(this, procedureName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public IProcedureQuery ProcedureQuery<TParameter>(string procedureName, TParameter parameter) where TParameter : class
        {
            return new ProcedureQuery(this, procedureName).AddParameters(parameter);
        }
        #endregion
    }
}
