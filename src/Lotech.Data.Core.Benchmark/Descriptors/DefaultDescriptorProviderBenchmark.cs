using BenchmarkDotNet.Attributes;

namespace Lotech.Data.Descriptors.Benchmark
{
    [CoreJob, MemoryDiagnoser, IterationCount(30), InvocationCount(10000)]
    public class DefaultDescriptorProviderBenchmark
    {
        [Benchmark]
        public void Complete()
        {
            DefaultDescriptorProvider.Instance.GetEntityDescriptor<DefaultDescriptorProviderBenchmark>(Operation.None);
        }
    }
}
