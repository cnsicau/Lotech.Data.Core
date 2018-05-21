using System;
using Lotech.Data.SqlServers;

namespace Lotech.Data.Example
{
    public class SqlServerExample : IDatabaseExample
    {
        IDatabase db = DatabaseFactory.CreateDatabase("sqlserver");

        public SqlServerExample()
        {
            db.ExecuteNonQuery("truncate table example");
        }

        IDatabase IDatabaseExample.Database => db;

        public PageData<Example> PageExecute(ISqlQuery query, Page page)
        {
            return query.PageExecuteEntites<Example>(page);
        }
    }
}
