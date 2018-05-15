using System;

namespace Lotech.Data.Example
{
    static class Program
    {
        static void EntityTests<TExample>(IDatabaseExample example)
            where TExample : class, IExample, new()
        {
            Console.WriteLine(("EntityTest " + example.GetType().Name).PadLeft(60, '-').PadRight(90, '-'));
            var test = new TestEntityExecutes<TExample>(example);
            test.TestInsert();
            test.TestLoad();
            test.TestUpdate();
            test.TestExists();
            test.TestDelete();
            test.TestTransaction();
            test.TestFind();
            test.TestCount();
            test.TestUpdate2();
        }

        static void SqlTests<TExample>(IDatabaseExample example)
            where TExample : class, IExample, new()
        {
            Console.WriteLine(("SqlTest " + example.GetType().Name).PadLeft(60, '-').PadRight(90, '-'));
            var test = new TestSqlExecutes<TExample>(example.Database);

            test.ExecuteDataSetTest();
            test.ExecuteEntitiesTest();
            test.ExecuteEntityTest();
            test.ExecuteDynamicTest();
            test.ExecuteScalarTest();
            test.ExecuteScalarTTest();
        }

        static void MethodTests<TExample>(IDatabaseExample example)
            where TExample : class, IExample, new()
        {
            Console.WriteLine(("MethodCall " + example.GetType().Name).PadLeft(60, '-').PadRight(90, '-'));                // MySQL
            new TestMethodCall<TExample>(example).Test();
        }

        static void Main()
        {
            // Entity
            var sqlite = new SQLiteExample();

            EntityTests<SQLiteExample.Example>(sqlite);    // SQLite 
            var mysql = new MySqlExample();
            EntityTests<Example>(mysql);                   // MySQL
            var oracle = new OracleExample();
            EntityTests<OracleExample.Example>(oracle);    // Oracle
            var sqlserver = new SqlServerExample();
            EntityTests<Example>(sqlserver);               // SqlServer
            var generic = new GenericExample();
            EntityTests<Example>(generic);                 // Generic
            // Methods
            MethodTests<SQLiteExample.Example>(sqlite);     // SQLite 
            MethodTests<Example>(mysql);                    // MySQL
            MethodTests<OracleExample.Example>(oracle);     // Oracle
            MethodTests<Example>(sqlserver);                // SqlServer
            //MethodTests<Example>(generic);                // Generic

            // Raw SQL
            SqlTests<SQLiteExample.Example>(sqlite);       // SQLite 
            SqlTests<Example>(mysql);                      // MySQL
            SqlTests<OracleExample.Example>(oracle);       // Oracle
            SqlTests<Example>(sqlserver);                  // SqlServer
            SqlTests<Example>(generic);                    // Generic
        }
    }
}
