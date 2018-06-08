using System;
using System.Linq;

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
            test.TestBug1();    // bug#1 fixed test

            var arr = new long[] { 1, 2 };
            var list = new System.Collections.Generic.List<long>(arr);
            list.Add(25);
            var srr = new string[] { "3", "4" };
            var el = example.Database.LoadEntity<Example>(_ => srr.Contains(_.Id.ToString()) && arr.Contains(_.Id) || list.Contains(_.Id));
            example.Database.Update<Example>().Set(new { Code = "C", Deleted = true }).Where(_ => _.Id == 5);
        }

        static void SqlTests<TExample>(IDatabaseExample example)
            where TExample : class, IExample, new()
        {
            Console.WriteLine(("SqlTest " + example.GetType().Name).PadLeft(60, '-').PadRight(90, '-'));
            var test = new TestSqlExecutes<TExample>(example);

            test.ExecuteDataSetTest();
            test.ExecuteEntitiesTest();
            test.ExecuteEntityTest();
            test.ExecuteDynamicTest();
            test.ExecuteScalarTest();
            test.ExecuteScalarTTest();
            test.SqlQueryTest();
        }

        static void MethodTests<TExample>(IDatabaseExample example)
            where TExample : class, IExample, new()
        {
            Console.WriteLine(("MethodCall " + example.GetType().Name).PadLeft(60, '-').PadRight(90, '-'));                // MySQL
            new TestMethodCall<TExample>(example).Test();
        }

        static void PageTests<TExample>(IDatabaseExample example)
            where TExample : class, IExample, new()
        {
            Console.WriteLine(("PageQuery " + example.GetType().Name).PadLeft(60, '-').PadRight(90, '-'));                // MySQL
            var test = new TestPageExecutes<TExample>(example);
            test.TestConcatQuery();
            test.Test();
        }

        static void Main()
        {
            // Entity
            var sqlite = new SQLiteExample();
            EntityTests<SQLiteExample.Example>(sqlite);    // SQLite 
            var mysql = new MySqlExample();
            EntityTests<Example>(mysql);                   // MySQL
            var oracle = new OracleExample();
            EntityTests<Example>(oracle);    // Oracle
            var sqlserver = new SqlServerExample();
            EntityTests<Example>(sqlserver);               // SqlServer
            var generic = new GenericExample();
            EntityTests<Example>(generic);                 // Generic
            // Methods
            MethodTests<SQLiteExample.Example>(sqlite);     // SQLite 
            MethodTests<Example>(mysql);                    // MySQL
            MethodTests<Example>(oracle);     // Oracle
            MethodTests<Example>(sqlserver);                // SqlServer
            //MethodTests<Example>(generic);                // Generic

            // Raw SQL
            SqlTests<SQLiteExample.Example>(sqlite);       // SQLite 
            SqlTests<Example>(mysql);                      // MySQL
            SqlTests<Example>(oracle);       // Oracle
            SqlTests<Example>(sqlserver);                  // SqlServer
            SqlTests<Example>(generic);                    // Generic

            // Page SQL
            PageTests<SQLiteExample.Example>(new SQLiteExample());      // SQLite 
            PageTests<Example>(new MySqlExample());                     // MySQL
            PageTests<Example>(new OracleExample());                    // Oracle
            PageTests<Example>(new SqlServerExample());                 // SqlServer
            PageTests<Example>(new GenericExample());                   // Generic
        }
    }
}
