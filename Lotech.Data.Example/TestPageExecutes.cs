using System;
using System.Collections.Generic;

namespace Lotech.Data.Example
{
    class TestPageExecutes<TExample> where TExample : class, IExample, new()
    {
        private readonly IDatabaseExample example;

        public TestPageExecutes(IDatabaseExample example)
        {
            this.example = example;
            ((DbProviderDatabase)example.Database).EnableTraceLog();
            this.example = example;
        }

        public void TestConcatQuery()
        {
            var query = example.Database.SqlQuery()
                .AppendLine("SELECT * FROM example")
                .AppendLine(" WHERE 1 = 1")
                .AppendLine("  AND {0} = {2} OR {3} = {1}", 0, 4, 2, 4);
            Console.WriteLine("query => " + query.GetSnippets());
            foreach (var p in query.GetParameters())
            {
                Console.WriteLine(" -- " + p.Key + " = " + p.Value);
            }
            // sub query
            var countQuery = example.Database.SqlQuery("SELECT COUNT(*) FROM (")
                        .Append(query)
                        .Append(") T");
            Console.WriteLine("countQuery => " + countQuery.GetSnippets());
            foreach (var p in countQuery.GetParameters())
            {
                Console.WriteLine(" -- " + p.Key + " = " + p.Value);
            }
        }

        public void Test()
        {
            var list = new List<TExample>();
            for (int i = 1; i <= 20; i++)
            {
                var ex = new TExample();
                ex.Code = "CD-" + i;
                ex.Content = new byte[1];
                ex.CreateTime = DateTime.Now;
                ex.Deleted = false;
                ex.Name = "测试" + i;
                list.Add(ex);
            }
            example.Database.InsertEntities(list);

            var query = example.Database.SqlQuery("SELECT * FROM Example WHERE 1 = 1")
                    .AppendNotNull(null, " AND 1 = NULL")
                    .AppendNotNull("%", " AND Code LIKE {0}")
                    .Append(" AND Name LIKE {0}", "%测试%");

            var p1 = new Page
            {
                Index = 0,
                Size = 1,
                Orders = new[]
                {
                    new PageOrder{ Column = "Id", Direction = PageOrderDirection.DESC }
                }
            };
            var data = example.PageExecute(query, p1);
            Console.WriteLine(" page 1: (20) = " + data?.Data?[0]?.Id);

            p1 = new Page
            {
                Index = 1,
                Size = 1,
                Orders = new[]
                {
                    new PageOrder{ Column = "Id", Direction = PageOrderDirection.DESC }
                }
            };

            data = example.PageExecute(query, p1);
            Console.WriteLine(" page 2: (19) = " + data?.Data?[0]?.Id);
        }
    }
}
