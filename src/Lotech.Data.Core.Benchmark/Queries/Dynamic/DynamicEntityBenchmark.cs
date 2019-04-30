using BenchmarkDotNet.Attributes;
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
        #region BenchmarkDataRecord
        class BenchmarkDataRecord : IDataRecord
        {
            private readonly string[] fields;
            private readonly object[] values;

            public BenchmarkDataRecord(string[] fields, object[] values)
            {
                this.fields = fields;
                this.values = values;
            }
            public object this[int i] => values[i];

            public object this[string name] => throw new NotImplementedException();

            public int FieldCount => fields.Length;

            public bool GetBoolean(int i)
            {
                throw new NotImplementedException();
            }

            public byte GetByte(int i)
            {
                throw new NotImplementedException();
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public char GetChar(int i)
            {
                throw new NotImplementedException();
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public IDataReader GetData(int i)
            {
                throw new NotImplementedException();
            }

            public string GetDataTypeName(int i)
            {
                throw new NotImplementedException();
            }

            public DateTime GetDateTime(int i)
            {
                throw new NotImplementedException();
            }

            public decimal GetDecimal(int i)
            {
                throw new NotImplementedException();
            }

            public double GetDouble(int i)
            {
                throw new NotImplementedException();
            }

            public Type GetFieldType(int i)
            {
                throw new NotImplementedException();
            }

            public float GetFloat(int i)
            {
                throw new NotImplementedException();
            }

            public Guid GetGuid(int i)
            {
                throw new NotImplementedException();
            }

            public short GetInt16(int i)
            {
                throw new NotImplementedException();
            }

            public int GetInt32(int i)
            {
                throw new NotImplementedException();
            }

            public long GetInt64(int i)
            {
                throw new NotImplementedException();
            }

            public string GetName(int i)
            {
                return fields[i];
            }

            public int GetOrdinal(string name)
            {
                throw new NotImplementedException();
            }

            public string GetString(int i)
            {
                throw new NotImplementedException();
            }

            public object GetValue(int i)
            {
                return values[i];
            }

            public int GetValues(object[] values)
            {
                Array.Copy(this.values, values, values.Length);
                return values.Length;
            }

            public bool IsDBNull(int i)
            {
                throw new NotImplementedException();
            }
        }

        class DynamicData : DynamicObject
        {
            readonly IDictionary<string, object> values = new Dictionary<string, object>();
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
        IDataRecord record;
        dynamic dynamicEntity;
        dynamic dynamicObject;
        dynamic dynamicData;

        [GlobalSetup]
        public void GlobalSetup()
        {
            record = new BenchmarkDataRecord(fields, new object[] { 1, "Lily", new byte[20] });

            dynamicEntity = new DynamicEntity(fields, new object[] { 1, "Lily", new byte[20] });

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
            new DynamicEntity(fields, new object[] { 1, "Lily", new byte[20] });
        }

        [Benchmark]
        public void VisitEntity()
        {
            Visit(dynamicEntity);
        }

        [Benchmark]
        public void CreateObject()
        {
            dynamic obj = new System.Dynamic.ExpandoObject();
            obj.Id = record.GetValue(0);
            obj.Name = record.GetValue(1);
            obj.Content = record.GetValue(2);
        }

        [Benchmark]
        public void VisitObject()
        {
            Visit(dynamicObject);
        }

        [Benchmark]
        public void CreateData()
        {
            dynamicData = new DynamicData();
            dynamicData.Id = record.GetValue(0);
            dynamicData.Name = record.GetValue(1);
            dynamicData.Content = record.GetValue(2);
        }

        [Benchmark]
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
