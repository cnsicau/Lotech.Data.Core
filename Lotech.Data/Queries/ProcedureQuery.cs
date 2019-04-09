using System.Collections.Generic;
using System.Data.Common;

namespace Lotech.Data.Queries
{
    class ProcedureQuery : Query, IProcedureQuery
    {
        readonly IList<KeyValuePair<string, object>> _parameters = new List<KeyValuePair<string, object>>();

        internal ProcedureQuery(IDatabase database) : base(database) { }

        internal ProcedureQuery(IDatabase database, string name) : base(database)
        {
            Name = name;
        }

        public string Name { get; set; }

        IProcedureQuery IProcedureQuery.AddParameter(string name, object value)
        {
            _parameters.Add(new KeyValuePair<string, object>(name, value));
            return this;
        }

        public override DbCommand CreateCommand()
        {
            var command = database.GetStoredProcedureCommand(Name);
            foreach (var p in _parameters)
            {
                var type = p.Value?.GetType() ?? typeof(string);
                database.AddInParameter(command, database.BuildParameterName(p.Key), database.ParseDbType(type), p.Value);
            }
            return command;
        }
    }
}
