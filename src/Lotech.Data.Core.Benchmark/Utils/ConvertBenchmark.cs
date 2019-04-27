using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lotech.Data.Utils.Benchmark
{
    [ShortRunJob, MemoryDiagnoser, IterationCount(30), InvocationCount(100000)]
    public class ConvertBenchmark
    {
        //[Params(null, "900", 800, 700D, 600F, 500L)]
        public object Value { get; set; }


        [Benchmark]
        public void ToDateTimeConvert()
        {
            DateTime.Parse("2018-10-11 19:07:08.987");
        }

        [Benchmark]
        public void CompleteDateTime()
        {
            Convert<DateTime>.From("2018-10-11 19:07:08.987");
            //Convert.ToDateTime("2018-10-11 19:07:08.987");
        }

        //[Benchmark]
        public void ToConvert()
        {
            var xt = Value is int ? (TypeCode)Value : Value == null ? default(TypeCode?) : (TypeCode)Convert.ToInt32(Value);
            //var t = Value is int ? (TypeCode)Value : (TypeCode)Convert.ToInt32(Value);
            //var j = Value is int ? (int)Value : Convert.ToInt32(Value);
            //var d = Value is double ? (double)Value : Convert.ToDouble(Value);
            //var f = Value is float ? (float)Value : Convert.ToSingle(Value);
            //var s = Value is short ? (short)Value : Convert.ToInt16(Value);
            //var l = Value is long ? (long)Value : Convert.ToInt64(Value);
            //var m = Value is decimal ? (decimal)Value : Convert.ToDecimal(Value);
        }

        //[Benchmark]
        public void ChangeConvert()
        {
            //var t = Value is int ? (TypeCode)Value : (TypeCode)Convert.ChangeType(Value, typeof(int));
            //var i = Value is int ? (int)Value : (int)Convert.ChangeType(Value, typeof(int));
            //var d = Value is double ? (double)Value : (double)Convert.ChangeType(Value, typeof(double));
            //var f = Value is float ? (float)Value : (float)Convert.ChangeType(Value, typeof(float));
            //var s = Value is short ? (short)Value : (short)Convert.ChangeType(Value, typeof(short));
            //var l = Value is long ? (long)Value : (long)Convert.ChangeType(Value, typeof(long));
            //var m = Value is decimal ? (decimal)Value : (decimal)Convert.ChangeType(Value, typeof(decimal));
        }

        //[Benchmark]
        public void Complete()
        {
            Convert<TypeCode?>.Func(Value);
            Convert<TypeCode>.Func(Value);
            Convert<int?>.Func(Value);
            Convert<int>.Func(Value);
            Convert<uint?>.Func(Value);
            Convert<uint>.Func(Value);
            Convert<string>.Func(Value);
        }
        //[Benchmark]
        public void CompleteEnumT()
        {
            Convert<TypeCode?>.Func(Value);
        }
        //[Benchmark]
        public void CompleteEnum()
        {
            Convert<TypeCode>.Func(Value);
        }
        //[Benchmark]
        public void CompleteInt32T()
        {
            Convert<int?>.Func(Value);
        }
        //[Benchmark]
        public void CompleteInt32()
        {
            Convert<int>.Func(Value);
        }
        //[Benchmark]
        public void CompleteUInt32T()
        {
            Convert<uint?>.Func(Value);
        }
        //[Benchmark]
        public void CompleteUInt32()
        {
            Convert<uint>.Func(Value);
        }
        //[Benchmark]
        public void CompleteString()
        {
            Convert<string>.Func(Value);
        }
    }
}
