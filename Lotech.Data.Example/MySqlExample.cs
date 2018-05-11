using System;
using System.Collections.Generic;

namespace Lotech.Data.Test
{
    public class MySqlExample
    {
        IDatabase db = DatabaseFactory.CreateDatabase("mysql");

        public MySqlExample()
        {
            Console.WriteLine(GetType().Name.PadLeft(40, '-').PadRight(80, '-'));

            db.ExecuteNonQuery("drop table if exists example");
            db.ExecuteNonQuery(@"create table Example (
    Id bigint not null auto_increment primary key,
    Code nvarchar(32) not null,
    Name nvarchar(128) not null,
    CreateTime datetime not null,
    ModifyTime datetime,
    Deleted bit not null,
    LongId bigint as ( createtime ) stored,
    Bin blob null
)");
        }

        public void TestInsert()
        {
            var example = new Example();
            example.Code = "T001";
            example.Name = "测试-01";
            example.CreateTime = DateTime.Now;
            example.Deleted = true;
            example.Content = new byte[1024];

            db.InsertEntity(example);
            Console.WriteLine("Insert example ID=" + example.Id);

            var examples = new List<Example>();
            for (int i = 0; i < 20; i++)
            {
                var t = new Example();
                t.Code = "T001";
                t.Name = "测试-01";
                t.CreateTime = DateTime.Now;
                t.Deleted = true;
                t.Content = new byte[1024];
                examples.Add(t);
            }
            db.InsertEntities(examples);
            examples.ForEach(_ => Console.WriteLine("Insert example ID=" + _.Id));
        }

        public void TestLoad()
        {
            var example = db.LoadEntity<Example, int>(1);
            Console.WriteLine("Load example by 1 Name=" + example.Name);

            example = db.LoadEntity(new Example { Id = 1 });
            Console.WriteLine("Load example by {Id=1}  Name=" + example.Name);

            example = db.LoadEntity<Example>(_ => _.Id <= 1);
            Console.WriteLine("Load example by (_=>_.Id <= 1)  Name=" + example.Name);

            example = db.LoadEntity<Example>(_ => _.Id == 99);
            Console.WriteLine("Load example by (_=>_.Id == 99)  Entity=" + example);
        }

        public void TestUpdate()
        {
            var example = db.LoadEntity<Example, int>(1);
            example.Name = Guid.NewGuid().ToString();
            db.UpdateEntity(example);

            var example2 = db.LoadEntity(new Example { Id = 1 });
            Console.WriteLine($"Example before {example.Name} {example2.Name}");

            example.Name = "O";
            example.Code = "CODE";
            db.UpdateEntityInclude(example, _ => new { _.Code });
            example2 = db.LoadEntity(new Example { Id = 1 });
            Console.WriteLine($"Example example.Name= {example.Name} example2.Name={example2.Name}");
            Console.WriteLine($"Example example.Code= {example.Code} example2.Code={example2.Code}");


            example.Code = "CODE999";
            db.UpdateEntityExclude(example, _ => new { _.Code });
            example2 = db.LoadEntity(new Example { Id = 1 });
            Console.WriteLine($"Example example.Name= {example.Name} example2.Name={example2.Name}");
            Console.WriteLine($"Example example.Code= {example.Code} example2.Code={example2.Code}");
        }

        public void TestExists()
        {
            var example = db.Exists<Example, int>(1);
            Console.WriteLine("Load example by 1 Exists=" + example);

            example = db.Exists(new Example { Id = 1 });
            Console.WriteLine("Load example by {Id=1}  Exists=" + example);

            example = db.Exists<Example>(_ => _.Id <= 1);
            Console.WriteLine("Load example by (_=>_.Id <= 1)  Exists=" + example);

            example = db.Exists<Example>(_ => _.Id == 99);
            Console.WriteLine("Load example by (_=>_.Id == 99)  Exists=" + example);
        }

        public void TestDelete()
        {
            var x = db.LoadEntity(new Example { Id = 1 });
            db.InsertEntity(x);
            db.DeleteEntity(x);
            Console.WriteLine($"delete example {x.Id} {(db.Exists(x) ? "failed" : "success")} ");

            db.InsertEntity(x);
            db.DeleteEntity<Example, long>(x.Id);
            Console.WriteLine($"delete example {x.Id} {(db.Exists(x) ? "failed" : "success")} ");

            db.InsertEntity(x);
            db.DeleteEntities<Example>(_ => _.Id == x.Id);
            Console.WriteLine($"delete example {x.Id} {(db.Exists(x) ? "failed" : "success")} ");

            var examples = new List<Example>();
            for (int i = 0; i < 20; i++)
            {
                examples.Add(new Example { Id = i + 1 });
            }
            db.DeleteEntities(examples);
            var id = examples[0].Id;
            Console.WriteLine($"delete example {x.Id} {(db.Exists<Example>(_ => _.Id == id) ? "failed" : "success")} ");
        }

        /// <summary>
        /// 
        /// </summary>
        public void TestTransaction()
        {
            var entity = db.LoadEntity<Example>(_ => _.Id > 0);
            // Commit Test
            using (var tm = new TransactionManager())
            {
                db.InsertEntity(entity);
                tm.Commit();
            }

            Console.WriteLine($"Transaction commit {(db.Exists(entity) ? "success" : "failed")}");

            // Rollback Test
            using (var tm = new TransactionManager())
            {
                db.InsertEntity(entity);
            }

            Console.WriteLine($"Transaction rollback {(!db.Exists(entity) ? "success" : "failed")}");
        }

        public void TestFind()
        {
            var entities = db.FindEntities<Example>();
            foreach (var e in entities)
            {
                Console.WriteLine($"Id: {e.Id}, Code: {e.Code}, Name: {e.Name}, LongId: {e.LongId}, CreateTime: {e.CreateTime}");
            }
        }
    }
}
