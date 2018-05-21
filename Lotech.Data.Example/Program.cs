using System;
using System.Collections.Generic;

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
            var test = new TestSqlExecutes<TExample>(example);

            test.ExecuteDataSetTest();
            test.ExecuteEntitiesTest();
            test.ExecuteEntityTest();
            test.ExecuteDynamicTest();
            test.ExecuteScalarTest();
            test.ExecuteScalarTTest();
            test.ExecutePageQueryTest();
        }

        static void MethodTests<TExample>(IDatabaseExample example)
            where TExample : class, IExample, new()
        {
            Console.WriteLine(("MethodCall " + example.GetType().Name).PadLeft(60, '-').PadRight(90, '-'));                // MySQL
            new TestMethodCall<TExample>(example).Test();
        }

        static void Main()
        {
            var x = new SQLiteExample();
            var list = new List<SQLiteExample.Example>();
            for (int i = 1; i <= 20; i++)
            {
                var ex = new SQLiteExample.Example();
                ex.Code = "CD-" + i;
                ex.Content = new byte[1];
                ex.CreateTime = DateTime.Now;
                ex.Deleted = false;
                ex.Name = "测试" + i;
                list.Add(ex);
            }
            x.Database.InsertEntities(list);
            new TestSqlExecutes<SQLiteExample.Example>(x).ExecutePageQueryTest();

            // Entity
            var sqlite = new SQLiteExample();
            EntityTests<SQLiteExample.Example>(sqlite);    // SQLite 

            var query = sqlite.Database.SqlQuery()
                .AppendLine("SELECT * FROM example")
                .AppendLine(" WHERE 1 = 1")
                .AppendLine("  AND {0} = {2} OR {3} = {1}", 0, 4, 2, 4);
            // sub query
            var countQuery = sqlite.Database.SqlQuery("SELECT COUNT(*) FROM (")
                        .Append(query)
                        .Append(") T");

            var q1 = query.ExecuteEntities();
            var q2 = query.ExecuteEntities<Example>();
            var q3 = query.ExecuteDataSet();


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
        }
    }
}
