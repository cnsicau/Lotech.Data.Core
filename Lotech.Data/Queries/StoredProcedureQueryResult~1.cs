using System;
using System.Collections.Generic;
using System.Data;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 存储过程查询结果
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TParameter"></typeparam>
    public class StoredProcedureQueryResult<TEntity, TParameter> : QueryResult<TEntity> where TParameter : class
    {
        private readonly IDbConnection _connection;
        private readonly TParameter _parameter;
        private readonly string _procedureName;
        private readonly int _timeout;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="mapper"></param>
        /// <param name="procedureName"></param>
        /// <param name="parameter"></param>
        /// <param name="log"></param>
        /// <param name="timeout"></param>
        public StoredProcedureQueryResult(IDbConnection connection, IResultMapper<TEntity> mapper, string procedureName, TParameter parameter, Action<string> log, int timeout) : base(mapper, log)
        {
            _connection = connection;
            _procedureName = procedureName;
            _parameter = parameter;
            _timeout = timeout;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator<TEntity> GetEnumerator()
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = _procedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = _timeout;

                var sp = new StoredProcedureParameter<TParameter>(_parameter, Log, ConvertParameterValue);
                Log("Execute StoredProcedure: " + _procedureName);
                sp.Binding(command);

                var enumerator = CreateEnumerator(command);
                sp.ReverseBinding();    // 将参数值反向输出
                return enumerator;
            }
        }
    }
}
