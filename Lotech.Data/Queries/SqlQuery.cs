using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace Lotech.Data.Queries
{
    [System.Diagnostics.DebuggerDisplay("{_snippets}")]
    class SqlQuery : ISqlQuery
    {
        #region Fields & Constructor
        /// <summary>
        /// 全局参数ID编号，以避免构建子查询时参数名可能重复问题
        /// </summary>
        [ThreadStatic]
        private static ushort _id = 0;

        private static readonly Regex _placeholder = new Regex(@"([^\{]?)\{\s*(\d+)s*\}([^\}]?)", RegexOptions.Multiline | RegexOptions.Compiled);

        private readonly StringBuilder _snippets = new StringBuilder();
        private readonly List<SqlQueryParameter> _parameters = new List<SqlQueryParameter>();
        private int _index;
        private IDatabase _database;

        /// <summary>
        /// 构造
        /// </summary>
        internal SqlQuery(IDatabase database)
        {
            _database = database;
            _id++;  // 随着SQLQuery对象增加
        }
        #endregion

        string NextParameterName() { return _database.BuildParameterName("p_" + _id + '_' + _index++); }

        IDatabase IQuery.Database
        {
            get { return _database; }
        }

        DbCommand IQuery.CreateCommand()
        {
            var command = _database.GetSqlStringCommand(_snippets.ToString());
            foreach (var p in _parameters)
            {
                _database.AddInParameter(command, p.Name, _database.ParseDbType(p.Type), p.Value);
            }
            return command;
        }

        ISqlQuery ISqlQuery.Append(string snippet)
        {
            _snippets.Append(snippet);
            return this;
        }

        ISqlQuery ISqlQuery.Append(string snippet, IList<object> args)
        {
            if (snippet == null) throw new ArgumentNullException(nameof(snippet));
            if (args == null) throw new ArgumentNullException(nameof(args));

            _snippets.Append(_placeholder.Replace(snippet, match =>
            {
                var groups = match.Groups;
                var placeIndex = Convert.ToInt16(groups[2].Value);

                if (placeIndex < 0 || placeIndex >= args.Count)
                    throw new InvalidOperationException($"未找到对应的参数值：{match.Value} ");

                var parameterName = NextParameterName();
                var value = args[placeIndex];
                if (value is SqlQueryParameter)
                {
                    _parameters.Add(new SqlQueryParameter(parameterName, ((SqlQueryParameter)value).Type, ((SqlQueryParameter)value).Value));
                }
                else
                {
                    _parameters.Add(new SqlQueryParameter(parameterName, value));
                }
                return groups[1].Value + parameterName + groups[3].Value;
            }));
            return this;
        }

        ISqlQuery ISqlQuery.AppendRaw(string snippet, IEnumerable<SqlQueryParameter> parameters)
        {
            _snippets.Append(snippet);
            _parameters.AddRange(parameters);
            return this;
        }

        string ISqlQuery.GetSnippets() { return _snippets.ToString(); }

        IEnumerable<SqlQueryParameter> ISqlQuery.GetParameters() { return _parameters; }
    }
}
