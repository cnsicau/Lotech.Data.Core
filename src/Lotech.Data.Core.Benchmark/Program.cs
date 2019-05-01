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
        static void Main(string[] args)
        {
            var b = new Queries.Benchmark.ObjectResultMapperBenchmark();
            b.GlobalSetup();
            b.Map();
            b.DapperMap();

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
