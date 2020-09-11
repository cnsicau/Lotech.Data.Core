using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 动态对象结果映射
    /// </summary>
    public class ObjectResultMapper : ResultMapper<object>
    {
        static readonly ConcurrentDictionary<RecordKey, IDictionary<string, int>> indexesCache = new ConcurrentDictionary<RecordKey, IDictionary<string, int>>();

        IDictionary<string, int> indexedColumns = null;

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
                    ^ record.GetName(0).GetHashCode()
                    ^ (record.FieldCount >= 2 ? record.GetName(record.FieldCount / 2).GetHashCode() : 0);
            }

            public IDictionary<string, int> CreateIndexes()
            {
                var indexes = new SortedList<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < record.FieldCount; i++)
                {
                    if(indexes.ContainsKey(record.GetName(i))) throw new InvalidOperationException(record.GetName(i) + "列名重复");
                    indexes[record.GetName(i)] = i;
                }
                record = new MetaRecord(record);
                return indexes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        public override void TearUp(IDataReader reader)
        {
            base.TearUp(reader);
            indexedColumns = indexesCache.GetOrAdd(new RecordKey(reader), key => key.CreateIndexes());
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
            var values = new object[indexedColumns.Count];
            reader.GetValues(values);
            result = new DynamicEntity(indexedColumns, values);
            return true;
        }
    }
}
