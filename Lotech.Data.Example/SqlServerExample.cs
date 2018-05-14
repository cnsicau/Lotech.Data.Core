using System;

namespace Lotech.Data.Example
{
    public class SqlServerExample : IDatabaseExample
    {
        IDatabase db = DatabaseFactory.CreateDatabase("sqlserver");

        public SqlServerExample()
        {
            Console.WriteLine(GetType().Name.PadLeft(40, '-').PadRight(80, '-'));

            db.ExecuteNonQuery("truncate table example");
        }

        IDatabase IDatabaseExample.Database => db;
    }
}
