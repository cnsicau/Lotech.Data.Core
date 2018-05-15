using System;

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
    }
}
