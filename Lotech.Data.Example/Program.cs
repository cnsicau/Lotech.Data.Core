using System;

namespace Lotech.Data.Example
{
    class Program
    {
        static void EntityTests<TExample>(IDatabaseExample example)
            where TExample : class, IExample, new()
        {
            var test = new TestEntityExecutes<TExample>(example);
            test.TestInsert();
            test.TestLoad();
            test.TestUpdate();
            test.TestExists();
            test.TestDelete();
            test.TestTransaction();
            test.TestFind();
        }

        static void SqlTests<TExample>(IDatabaseExample example)
        {
            Console.WriteLine(example.GetType().Name.PadLeft(40, '-').PadRight(80, '-'));
            var test = new TestSqlExecutes(example.Database);

            test.ExecuteDataSetTest();
            test.ExecuteNonQueryTest();
        }

        static void Main()
        {
            var sqlite = new SQLiteExample();
            // Entity
            EntityTests<SQLiteExample.Example>(sqlite);    // SQLite 
            var mysql = new MySqlExample();
            EntityTests<Example>(mysql);                   // MySQL
            var oracle = new OracleExample();
            EntityTests<OracleExample.Example>(oracle);    // Oracle
            var sqlserver = new SqlServerExample();
            EntityTests<Example>(sqlserver);               // SqlServer
            var generic = new GenericExample();
            EntityTests<Example>(generic);                 // Generic

            // Raw SQL
            SqlTests<SQLiteExample.Example>(sqlite);       // SQLite 
            SqlTests<Example>(mysql);                      // MySQL
            SqlTests<OracleExample.Example>(oracle);       // Oracle
            SqlTests<Example>(sqlserver);                  // SqlServer
            SqlTests<Example>(generic);                    // Generic
        }
    }
}
