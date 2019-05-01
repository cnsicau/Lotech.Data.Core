using BenchmarkDotNet.Attributes;
using Lotech.Data.Benchmark;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Text;

namespace Lotech.Data.Queries.Dynamic.Benchmark
{
    /// <summary>
    /// 
    /// </summary>
    [ShortRunJob, MemoryDiagnoser]
    public class DynamicEntityBenchmark
    {
        #region DynamicData
        class DynamicData : DynamicObject
        {
            readonly IDictionary<string, object> values = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                values[binder.Name] = value;
                return true;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                return values.TryGetValue(binder.Name, out result);
            }
        }
        #endregion

        string[] fields = new[] { "Id", "Name", "Content" };
        object[] values = new object[] { 1, "Lily", new byte[20] };
        IDataRecord record;
        dynamic dynamicEntity;
        dynamic dynamicObject;
        dynamic dynamicData;

        [GlobalSetup]
        public void GlobalSetup()
        {
            record = new BenchmarkDataReader(fields, values);

            dynamicEntity = new DynamicEntity(fields, values);

            dynamicObject = new ExpandoObject();
            dynamicObject.Id = record.GetValue(0);
            dynamicObject.Name = record.GetValue(1);
            dynamicObject.Content = record.GetValue(2);

            dynamicData = new DynamicData();
            dynamicData.Id = record.GetValue(0);
            dynamicData.Name = record.GetValue(1);
            dynamicData.Content = record.GetValue(2);
        }

        [Benchmark]
        public void CreateEntity()
        {
            new DynamicEntity(fields, values);
        }

        [Benchmark]
        public void ConvertEntity()
        {
            var dnt = (BenchmarkDataModel)dynamicEntity;
        }

        //[Benchmark]
        public void VisitEntity()
        {
            Visit(dynamicEntity);
        }

        //[Benchmark]
        public void CreateObject()
        {
            dynamic obj = new System.Dynamic.ExpandoObject();
            obj.Id = record.GetValue(0);
            obj.Name = record.GetValue(1);
            obj.Content = record.GetValue(2);
        }

        //[Benchmark]
        public void VisitObject()
        {
            Visit(dynamicObject);
        }

        //[Benchmark]
        public void CreateData()
        {
            dynamicData = new DynamicData();
            dynamicData.Id = record.GetValue(0);
            dynamicData.Name = record.GetValue(1);
            dynamicData.Content = record.GetValue(2);
        }

        //[Benchmark]
        public void VisitData()
        {
            Visit(dynamicData);
        }

        static void Visit(dynamic val)
        {
            if (val.Id != 1) throw new Exception();
            if (val.Name != "Lily") throw new Exception();
            if (!(val.Content is byte[])) throw new Exception();
        }
    }
}
