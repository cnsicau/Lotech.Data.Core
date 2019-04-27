using BenchmarkDotNet.Running;
using System;

namespace Lotech.Data.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var benchmark = new SQLiteDatabaseBenchmark();
            benchmark.Count = 1;
            benchmark.TearUp();
            benchmark.Raw();
            benchmark.Complete();

            //BenchmarkRunner.Run<Utils.Benchmark.DbTypeParserBenchmark>();
            //BenchmarkRunner.Run<Utils.Benchmark.ConvertBenchmark>();
            //BenchmarkRunner.Run<Descriptors.Benchmark.DefaultDescriptorProviderBenchmark>();
            BenchmarkRunner.Run<SQLiteDatabaseBenchmark>();
        }
    }
}
