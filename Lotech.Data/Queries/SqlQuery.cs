﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace Lotech.Data.Queries
{
    class SqlQuery : ISqlQuery
    {
        #region Fields & Constructor
        private static readonly Regex _placeholder = new Regex(@"([^\{]?)\{\s*(\d+)s*\}([^\}]?)", RegexOptions.Multiline | RegexOptions.Compiled);

        private readonly StringBuilder _snippets = new StringBuilder();
        private readonly SortedList<string, object> _parameters = new SortedList<string, object>();
        private int _index;
        private IDatabase _database;

        /// <summary>
        /// 构造
        /// </summary>
        internal SqlQuery(IDatabase database)
        {
            _database = database;
        }
        #endregion

        string NextParameterName() { return _database.BuildParameterName("p_query_" + _index++); }

        IDatabase IQuery.Database
        {
            get { return _database; }
        }

        DbCommand IQuery.CreateCommand()
        {
            var command = _database.GetSqlStringCommand(_snippets.ToString());

            foreach (var p in _parameters)
            {
                var type = p.Value?.GetType() ?? typeof(string);
                _database.AddInParameter(command, p.Key, Utils.DbTypeParser.Parse(type), p.Value);
            }
            return command;
        }

        ISqlQuery ISqlQuery.Append(string snippet)
        {
            _snippets.Append(snippet);
            return this;
        }

        ISqlQuery ISqlQuery.Append(string snippet, params object[] args)
        {
            if (snippet == null) throw new ArgumentNullException(nameof(snippet));
            if (args == null) throw new ArgumentNullException(nameof(args));

            _snippets.Append(_placeholder.Replace(snippet, match =>
            {
                var groups = match.Groups;
                var placeIndex = Convert.ToInt16(groups[2].Value);

                if (placeIndex < 0 || placeIndex >= args.Length)
                    throw new InvalidOperationException($"未找到对应的参数值：{match.Value} ");

                var parameterName = NextParameterName();
                _parameters.Add(parameterName, args[placeIndex]);
                return groups[1].Value + parameterName + groups[3].Value;
            }));
            return this;
        }

        string ISqlQuery.GetSnippets() { return _snippets.ToString(); }

        IEnumerable<KeyValuePair<string, object>> ISqlQuery.GetParameters() { return _parameters; }
    }
}
