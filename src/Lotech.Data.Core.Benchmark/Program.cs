using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace Lotech.Data.Benchmark
{
    class Program
    {
        class XT
        {
            public byte Id { get; set; }

            public string Name;
        }
        static void Main(string[] args)
        {
            var benchmark = new SQLiteDatabaseBenchmark();
            benchmark.Count = 20;
            benchmark.TearUp();
            benchmark.CompleteDynamicOne();
            benchmark.DapperDynamicOne();
            var entities = benchmark.database.ExecuteEntities("SELECT * FROM BenchmarkDataModel");
            foreach (var entity in entities)
            {
                XT xt = entity;
                IDictionary<string, object> dic = entity;
                IEnumerable<KeyValuePair<string, object>> pairs = entity;
                Console.WriteLine("{0} {1} {2} {3}\n  {4}", entity.id, entity.CODE, entity.Name, entity.Deleted,
                        JsonConvert.SerializeObject(entity));
            }
            return;
            //BenchmarkRunner.Run<Utils.Benchmark.DbTypeParserBenchmark>();
            //BenchmarkRunner.Run<Utils.Benchmark.ConvertBenchmark>();
            //BenchmarkRunner.Run<Descriptors.Benchmark.DefaultDescriptorProviderBenchmark>();
            BenchmarkRunner.Run<SQLiteDatabaseBenchmark>();
            //BenchmarkRunner.Run<SQLDatabaseBenchmark>();
            //BenchmarkRunner.Run<Queries.Dynamic.Benchmark.DynamicEntityBenchmark>();
            //BenchmarkRunner.Run<Queries.Benchmark.ObjectResultMapperBenchmark>();
        }
    }
}
