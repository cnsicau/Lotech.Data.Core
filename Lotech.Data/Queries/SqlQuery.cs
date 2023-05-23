using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Lotech.Data.Queries
{
    [System.Diagnostics.DebuggerDisplay("{_snippets}")]
    class SqlQuery : Query, ISqlQuery
    {
        #region Fields & Constructor
        /// <summary>
        /// 全局参数ID编号，以避免构建子查询时参数名可能重复问题
        /// </summary>
        [ThreadStatic]
        static int global_id;

        private readonly StringBuilder _snippets;
        private readonly List<SqlQueryParameter> _parameters = new List<SqlQueryParameter>(4);
        private int _index;

        /// <summary>
        /// 构造
        /// </summary>
        internal SqlQuery(IDatabase database) : this(database, 128) { }

        /// <summary>
        /// 构造
        /// </summary>
        internal SqlQuery(IDatabase database, int capacity) : base(database)
        {
            _snippets = new StringBuilder(capacity);
            _index = (global_id++ & 0x7FF) << 20; ;  // 随着SQLQuery对象增加
        }

        /// <summary>
        /// 构造
        /// </summary>
        internal SqlQuery(IDatabase database, string sql) : base(database)
        {
            _snippets = new StringBuilder(sql);
            _index = (global_id++ & 0x7FF) << 20; ;  // 随着SQLQuery对象增加
        }

        /// <summary>
        /// 构造
        /// </summary>
        internal SqlQuery(IDatabase database, string sql, int offset, int length) : base(database)
        {
            _snippets = new StringBuilder(sql, offset, length, 256);
            _index = (global_id++ & 0x7FF) << 20; ;  // 随着SQLQuery对象增加
        }
        #endregion

        public override DbCommand CreateCommand()
        {
            var command = database.GetSqlStringCommand(_snippets.ToString());
            foreach (var p in _parameters)
            {
                database.AddInParameter(command, p.Name, database.ParseDbType(p.Type), p.Value);
            }
            return command;
        }

        string ISqlQuery.NextParameterName()
        {
            return database.BuildParameterName("p"
                                + (_index >> 20)
                                + "_"
                                + (_index++ & 0xFFFFF)
                            );
        }

        ISqlQuery ISqlQuery.AppendString(string snippet, int offset, int length)
        {
            _snippets.Append(snippet, offset, length);
            return this;
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

            int enterIndex = -1, placeIndex = -1, index = 0;
            bool exit = false, quota = false;
            for (int i = 0; i < snippet.Length; i++)
            {
                var c = snippet[i];
                if (c == '\'') quota = !quota;

                if (!quota && enterIndex >= 0)
                {
                    if (c == '}')
                    {
                        if (placeIndex >= 0)
                        {
                            var parameterName = ((ISqlQuery)this).NextParameterName();
                            _snippets.Append(snippet, index, enterIndex - index).Append(parameterName);

                            var value = args[placeIndex];
                            if (value is SqlQueryParameter)
                            {
                                _parameters.Add(new SqlQueryParameter(parameterName, ((SqlQueryParameter)value).Type, ((SqlQueryParameter)value).Value));
                            }
                            else
                            {
                                _parameters.Add(new SqlQueryParameter(parameterName, value));
                            }
                            index = i + 1;
                        }
                        enterIndex = -1;
                    }
                    else if (c == ' ') { if (placeIndex >= 0) exit = true; }
                    else if (c >= '0' && c <= '9' && !exit)
                    {
                        placeIndex = placeIndex == -1 ? (c - '0') : (placeIndex * 10) + (c - '0');
                    }
                    else if (c == '{')
                    {
                        enterIndex = i;
                        placeIndex = -1;
                        exit = false;
                    }
                    else { enterIndex = -1; }
                }
                else if (c == '{')
                {
                    enterIndex = i;
                    placeIndex = -1;
                    exit = false;
                }
            }
            _snippets.Append(snippet, index, snippet.Length - index);
            return this;
        }

        ISqlQuery ISqlQuery.AppendRaw(string snippet, IEnumerable<SqlQueryParameter> parameters)
        {
            _snippets.Append(snippet);
            _parameters.AddRange(parameters);
            return this;
        }

        ISqlQuery ISqlQuery.AppendRaw(string snippet, int offset, int length, IEnumerable<SqlQueryParameter> parameters)
        {
            _snippets.Append(snippet, offset, length);
            _parameters.AddRange(parameters);
            return this;
        }

        string ISqlQuery.GetSnippets() { return _snippets.ToString(); }

        IEnumerable<SqlQueryParameter> ISqlQuery.GetParameters() { return _parameters; }
    }
}
