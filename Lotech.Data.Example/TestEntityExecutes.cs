using System;
using System.Collections.Generic;
using System.Text;

namespace Lotech.Data.Example
{
    class TestEntityExecutes<TExample> where TExample : class, IExample, new()
    {

        private readonly IDatabase db;

        public TestEntityExecutes(IDatabaseExample example)
        {
            this.db = example.Database;
            ((DbProviderDatabase)db).EnableTraceLog();
        }

        public void TestInsert()
        {
            var example = new TExample();
            example.Code = "T001";
            example.Name = "测试-01";
            example.CreateTime = DateTime.Now;
            example.Deleted = true;
            example.Content = new byte[1024];

            db.InsertEntity(example);
            Console.WriteLine("Insert example ID=" + example.Id);

            var examples = new List<TExample>();
            for (int i = 0; i < 20; i++)
            {
                var t = new TExample();
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

        internal void TestUpdate2()
        {
            var entity = new TExample();
            entity.Code = "9999";
            entity.ModifyTime = DateTime.Now;
            entity.Name = "Invalid";
            entity.Deleted = false;

            var before = db.FindEntities<TExample>(_ => _.Code.StartsWith("T"));
            foreach (var i in before)
                Console.WriteLine($"Before update name={i.Name} code={i.Code} deleted = {i.Deleted} modifytime={i.ModifyTime}");

            db.UpdateEntities(entity
                , _ => new { _.Deleted, _.ModifyTime }
                , _ => _.Code.StartsWith("T"));

            var after = db.FindEntities<TExample>(_ => _.Code.StartsWith("T"));
            foreach (var i in after)
                Console.WriteLine($"After update name={i.Name} code={i.Code} deleted = {i.Deleted} modifytime={i.ModifyTime}");
        }

        public void TestLoad()
        {
            var example = db.LoadEntity<TExample, int>(1);
            Console.WriteLine("Load example by 1 Name=" + example.Name);

            example = db.LoadEntity(new TExample { Id = 1 });
            Console.WriteLine("Load example by {Id=1}  Name=" + example.Name);

            example = db.LoadEntity<TExample>(_ => _.Id <= 1);
            Console.WriteLine("Load example by (_=>_.Id <= 1)  Name=" + example.Name);

            example = db.LoadEntity<TExample>(_ => _.Id == 99);
            Console.WriteLine("Load example by (_=>_.Id == 99)  Entity=" + example);
        }

        public void TestUpdate()
        {
            var example = db.LoadEntity<TExample, int>(1);
            example.Name = Guid.NewGuid().ToString();
            db.UpdateEntity(example);

            var example2 = db.LoadEntity(new TExample { Id = 1 });
            Console.WriteLine($"Example before {example.Name} {example2.Name}");

            example.Name = "O";
            example.Code = "CODE";
            db.UpdateEntityInclude(example, _ => new { _.Code });
            example2 = db.LoadEntity(new TExample { Id = 1 });
            Console.WriteLine($"Example example.Name= {example.Name} example2.Name={example2.Name}");
            Console.WriteLine($"Example example.Code= {example.Code} example2.Code={example2.Code}");


            example.Code = "CODE999";
            db.UpdateEntityExclude(example, _ => new { _.Code });
            example2 = db.LoadEntity(new TExample { Id = 1 });
            Console.WriteLine($"Example example.Name= {example.Name} example2.Name={example2.Name}");
            Console.WriteLine($"Example example.Code= {example.Code} example2.Code={example2.Code}");
        }

        public void TestExists()
        {
            var example = db.Exists<TExample, int>(1);
            Console.WriteLine("Load example by 1 Exists=" + example);

            example = db.Exists(new TExample { Id = 1 });
            Console.WriteLine("Load example by {Id=1}  Exists=" + example);

            example = db.Exists<TExample>(_ => _.Id <= 1);
            Console.WriteLine("Load example by (_=>_.Id <= 1)  Exists=" + example);

            example = db.Exists<TExample>(_ => _.Id == 99);
            Console.WriteLine("Load example by (_=>_.Id == 99)  Exists=" + example);
        }

        public void TestDelete()
        {
            var x = db.LoadEntity(new TExample { Id = 1 });
            db.InsertEntity(x);
            db.DeleteEntity(x);
            Console.WriteLine($"delete example {x.Id} {(db.Exists(x) ? "failed" : "success")} ");

            db.InsertEntity(x);
            db.DeleteEntity<TExample, long>(x.Id);
            Console.WriteLine($"delete example {x.Id} {(db.Exists(x) ? "failed" : "success")} ");

            db.InsertEntity(x);
            db.DeleteEntities<TExample>(_ => _.Id == x.Id && _.LongId == x.LongId);
            Console.WriteLine($"delete example {x.Id} {(db.Exists(x) ? "failed" : "success")} ");

            var examples = new List<TExample>();
            for (int i = 0; i < 20; i++)
            {
                examples.Add(new TExample { Id = i + 1 });
            }
            db.DeleteEntities(examples);
            var id = examples[0].Id;
            Console.WriteLine($"delete example {x.Id} {(db.Exists<TExample>(_ => _.Id == id) ? "failed" : "success")} ");
        }

        /// <summary>
        /// 
        /// </summary>
        public void TestTransaction()
        {
            var entity = db.LoadEntity<TExample>(_ => _.Id > 0);
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
            var entities = db.FindEntities<TExample>();
            foreach (var e in entities)
            {
                Console.WriteLine($"FindAll() => Id: {e.Id}, Code: {e.Code}, Name: {e.Name}, LongId: {e.LongId}, CreateTime: {e.CreateTime}");
            }
            entities = db.FindEntities<TExample>(_ => _.Code.StartsWith("T"));
            foreach (var e in entities)
            {
                Console.WriteLine($"Find(_.Code.StartsWith(\"T\")) => Id: {e.Id}, Code: {e.Code}, Name: {e.Name}, LongId: {e.LongId}, CreateTime: {e.CreateTime}");
            }
        }

        public void TestCount()
        {
            var count = db.Count<TExample>();
            Console.WriteLine("Count() = " + count);
            count = db.Count<TExample>(_ => _.Code.StartsWith("T"));
            Console.WriteLine("Count(_ => _.Code.StartsWith(\"T\")) = " + count);
        }
    }
}
