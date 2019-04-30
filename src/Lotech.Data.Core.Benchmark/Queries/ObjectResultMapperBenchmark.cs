using BenchmarkDotNet.Attributes;
using Lotech.Data.Queries.Dynamic;
using System;
using System.Data;

namespace Lotech.Data.Queries.Benchmark
{
    [ShortRunJob, MemoryDiagnoser]
    public class ObjectResultMapperBenchmark
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
        #endregion


        IDataRecord record;

        [GlobalSetup]
        public void GlobalSetup()
        {
            record = new BenchmarkDataRecord(new[] {
"id",
"distrib_plan_id",
"distrib_plan_code",
"distrib_dtl_code",
"product_id",
"product_code",
"product_name",
"station_code",
"station_name",
"station_pot_id",
"station_pot_num",
"conveyance_tank_id",
"conveyance_tank_num",
"retail_pot_id",
"retail_tank_id",
"quantity",
"quantity_uom",
"retail_quantity",
"retail_qty_uom",
"gen_type",
"distance",
"distance_uom",
"retail_distance",
"retail_dist_uom",
"finish_date",
"is_finish",
"quantity_switch",
"quantity_uom_switch",
"station_id",
"waybill_id",
"order_number",
"insertdate",
"insertdate_partition"}, new object[33]);
        }

        [Benchmark]
        public void MapperInitialize()
        {
            new ObjectResultMapper().Initialize(null, record);
        }

        [Benchmark]
        public void RawColumns()
        {
            var columns = new string[record.FieldCount];
            for (int i = 0; i < record.FieldCount; i++)
            {
                columns[i] = record.GetName(i);
            }
        }
    }
}
