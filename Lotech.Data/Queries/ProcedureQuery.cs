using System.Collections.Generic;
using System.Data.Common;

namespace Lotech.Data.Queries
{
    class ProcedureQuery : IProcedureQuery
    {
        readonly IList<KeyValuePair<string, object>> _parameters = new List<KeyValuePair<string, object>>();
        private readonly IDatabase _database;

        internal ProcedureQuery(IDatabase database)
        {
            _database = database;
        }

        internal ProcedureQuery(IDatabase database, string name)
        {
            _database = database;
            Name = name;
        }

        public string Name { get; set; }

        IProcedureQuery IProcedureQuery.AddParameter(string name, object value)
        {
            _parameters.Add(new KeyValuePair<string, object>(name, value));
            return this;
        }

        DbCommand IQuery.CreateCommand()
        {
            var command = _database.GetStoredProcedureCommand(Name);
            foreach (var p in _parameters)
            {
                var type = p.Value?.GetType() ?? typeof(string);
                _database.AddInParameter(command, _database.BuildParameterName(p.Key), Utils.DbTypeParser.Parse(type), p.Value);
            }
            return command;
        }

        IDatabase IQuery.Database { get { return _database; } }
    }
}
