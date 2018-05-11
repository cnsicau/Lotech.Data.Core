using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// SQL查询结果
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class SqlQueryResult<TEntity> : QueryResult<TEntity>
    {
        static readonly Regex placeholder = new Regex(@"(?<S>[^\{]?)\{\s*(?<Place>\d+)s*\}(?<E>[^\}]?)", RegexOptions.Multiline | RegexOptions.Compiled);

        private readonly IDbConnection _connection;
        private readonly string _sql;
        private readonly object[] _parameters;
        private readonly Func<string, string> _parameterNaming;
        private readonly int _timeout;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection">连接</param>
        /// <param name="sql">SQL</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="parameterNaming">参数名构建器</param>
        /// <param name="mapper">数据行映射器</param>
        /// <param name="log">日志回调</param>
        /// <param name="timeout">超时</param>
        public SqlQueryResult(IDbConnection connection, string sql, object[] parameters, Func<string, string> parameterNaming, IResultMapper<TEntity> mapper, Action<string> log, int timeout)
            : base(mapper, log)
        {
            _connection = connection;
            _sql = sql;
            _parameterNaming = parameterNaming;
            _parameters = parameters;
            _timeout = timeout;
        }

        string CompileCommandText(IDbCommand command, out Dictionary<int, int> outputMappings)
        {
            outputMappings = new Dictionary<int, int>();
            var mapping = outputMappings;

            return placeholder.Replace(_sql, m =>
            {
                var parameterIndex = int.Parse(m.Groups["Place"].Value);
                if (_parameters == null || parameterIndex < 0 || parameterIndex >= _parameters.Length)
                    throw new IndexOutOfRangeException(m.Value + "超出参数范围");

                var value = _parameters[parameterIndex];

                if (value is IRawParameterValue)    // 处理原始值替换
                {
                    var rawValue = ((IRawParameterValue)value).Value;
                    return m.Groups["S"].Value + rawValue?.ToString().Replace("'", "") + m.Groups["E"].Value;
                }

                var parameterName = _parameterNaming("p_sql_" + parameterIndex);
                var parameterValue = ConvertParameterValue(value);
                var commandParameter = command.CreateParameter();
                commandParameter.Direction = ParameterDirection.InputOutput;
                commandParameter.Value = parameterValue;
                FixDateParameterType(commandParameter, parameterValue);
                commandParameter.ParameterName = parameterName;

                mapping[command.Parameters.Count] = parameterIndex;
                command.Parameters.Add(commandParameter);

                return m.Groups["S"].Value + parameterName + m.Groups["E"].Value;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator<TEntity> GetEnumerator()
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                // Log("Prepare SqlQuery: " + _sql); 
                Dictionary<int, int> outputMappings;
                command.CommandText = CompileCommandText(command, out outputMappings);
                command.CommandTimeout = _timeout;

                Log("Executing SqlQuery: \n" + command.CommandText);
                foreach (DbParameter parameter in command.Parameters)
                    Log(" -- " + parameter.ParameterName + " = " + parameter.Value + "\t" + (parameter.Value ?? DBNull.Value).GetType().Name);

                var enumerator = CreateEnumerator(command);
                foreach (var reverse in outputMappings)                 // 反向输出参数
                    _parameters[reverse.Value] = ((DbParameter)command.Parameters[reverse.Key]).Value;

                return enumerator;
            }
        }
    }
}
