using System;
using System.Collections.Concurrent;
using System.Data;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 动态对象结果映射
    /// </summary>
    public class ObjectResultMapper : ResultMapper<object>
    {
        static readonly ConcurrentDictionary<RecordKey, string[]> columnsMeta = new ConcurrentDictionary<RecordKey, string[]>();

        string[] columns;

        class RecordKey
        {
            private IDataRecord record;

            public RecordKey(IDataRecord record) { this.record = record; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                if (record.FieldCount == ((RecordKey)obj).record.FieldCount
                        && record.GetName(0) == ((RecordKey)obj).record.GetName(0))
                {
                    for (int i = record.FieldCount - 1; i > 0; i--)
                    {
                        if (record.GetName(i) != ((RecordKey)obj).record.GetName(i)) return false;
                    }
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return record.FieldCount
                    ^ record.GetName(0).GetHashCode();
            }

            public string[] Strip()
            {
                record = new MetaRecord(record);
                return ((MetaRecord)record).Columns;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        public override void TearUp(IDataReader reader)
        {
            base.TearUp(reader);
            columns = columnsMeta.GetOrAdd(new RecordKey(reader), key => key.Strip());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool MapNext(out dynamic result)
        {
            if (!reader.Read())
            {
                result = null;
                return false;
            }
            var values = new object[columns.Length];
            reader.GetValues(values);
            // DBNull to null
            for (int i = values.Length - 1; i >= 0; i--)
            {
                if (values[i] == DBNull.Value) values[i] = null;
            }
            result = new DynamicEntity(columns, values);
            return true;
        }
    }
}
