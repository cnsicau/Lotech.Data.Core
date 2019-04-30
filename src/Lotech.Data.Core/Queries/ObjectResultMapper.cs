using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq.Expressions;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 动态对象结果映射
    /// </summary>
    class ObjectResultMapper : ResultMapper<object>
    {
        static readonly ConcurrentDictionary<RecordKey, string[]> columnsMeta = new ConcurrentDictionary<RecordKey, string[]>();

        string[] columns;
        class MetaRecord : IDataRecord
        {
            private readonly string[] columns;

            public string[] Columns { get { return columns; } }

            #region Constructor

            internal MetaRecord(IDataRecord record)
            {
                columns = new string[record.FieldCount];
                for (int i = 0; i < record.FieldCount; i++)
                {
                    columns[i] = record.GetName(i);
                }
            }
            #endregion

            #region IDataRecord

            int IDataRecord.FieldCount => columns.Length;

            object IDataRecord.this[string name] { get { throw new NotImplementedException(); } }

            object IDataRecord.this[int i] { get { throw new NotImplementedException(); } }

            bool IDataRecord.GetBoolean(int i) { throw new NotImplementedException(); }

            byte IDataRecord.GetByte(int i) { throw new NotImplementedException(); }

            long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) { throw new NotImplementedException(); }

            char IDataRecord.GetChar(int i) { throw new NotImplementedException(); }

            long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            IDataReader IDataRecord.GetData(int i) { throw new NotImplementedException(); }

            string IDataRecord.GetDataTypeName(int i) { throw new NotImplementedException(); }

            DateTime IDataRecord.GetDateTime(int i) { throw new NotImplementedException(); }

            decimal IDataRecord.GetDecimal(int i) { throw new NotImplementedException(); }

            double IDataRecord.GetDouble(int i) { throw new NotImplementedException(); }

            Type IDataRecord.GetFieldType(int i) { throw new NotImplementedException(); }

            float IDataRecord.GetFloat(int i) { throw new NotImplementedException(); }

            Guid IDataRecord.GetGuid(int i) { throw new NotImplementedException(); }

            short IDataRecord.GetInt16(int i) { throw new NotImplementedException(); }

            int IDataRecord.GetInt32(int i) { throw new NotImplementedException(); }

            long IDataRecord.GetInt64(int i) { throw new NotImplementedException(); }

            string IDataRecord.GetName(int i) => columns[i];

            int IDataRecord.GetOrdinal(string name) { throw new NotImplementedException(); }

            string IDataRecord.GetString(int i) { throw new NotImplementedException(); }

            object IDataRecord.GetValue(int i) { throw new NotImplementedException(); }

            int IDataRecord.GetValues(object[] values) { throw new NotImplementedException(); }

            bool IDataRecord.IsDBNull(int i) { throw new NotImplementedException(); }
            #endregion
        }


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
        /// <param name="record"></param>
        public override void Initialize(IDatabase database, IDataRecord record)
        {
            columns = columnsMeta.GetOrAdd(new RecordKey(record), key => key.Strip());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public override dynamic Map(IDataRecord record)
        {
            var values = new object[columns.Length];
            record.GetValues(values);
            return new DataExpando(columns, values);
        }

        #region DataExpando
        /// <summary>
        /// 
        /// </summary>
        public class DataExpando : IDictionary<string, object>, IDynamicMetaObjectProvider
        {
            private readonly string[] keys;
            private readonly object[] values;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="keys"></param>
            /// <param name="values"></param>
            public DataExpando(string[] keys, object[] values)
            {
                this.keys = keys;
                this.values = values;
            }

            #region IDictionary<string, object>
            object IDictionary<string, object>.this[string key]
            {
                get { return GetValue(key); }
                set { throw new NotImplementedException(); }
            }

            ICollection<string> IDictionary<string, object>.Keys => keys;

            ICollection<object> IDictionary<string, object>.Values => values;

            int ICollection<KeyValuePair<string, object>>.Count => keys.Length;

            bool ICollection<KeyValuePair<string, object>>.IsReadOnly => true;

            void IDictionary<string, object>.Add(string key, object value) { throw new ReadOnlyException(); }

            void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) { throw new ReadOnlyException(); }

            void ICollection<KeyValuePair<string, object>>.Clear() { throw new ReadOnlyException(); }

            bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            bool IDictionary<string, object>.ContainsKey(string key)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i].Equals(key, StringComparison.InvariantCultureIgnoreCase)) return true;
                }
                return false;
            }

            void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    array[arrayIndex + i] = new KeyValuePair<string, object>(keys[i], values[i]);
                }
            }

            IEnumerable<KeyValuePair<string, object>> GetKeyValuePairs()
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    yield return new KeyValuePair<string, object>(keys[i], values[i]);
                }
            }

            IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
            {
                return GetKeyValuePairs().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetKeyValuePairs().GetEnumerator();
            }

            bool IDictionary<string, object>.Remove(string key) { throw new ReadOnlyException(); }

            bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) { throw new ReadOnlyException(); }

            bool IDictionary<string, object>.TryGetValue(string key, out object value)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i].Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        value = values[i];
                        return true;
                    }
                }
                value = null;
                return false;
            }
            public object GetValue(string name)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i].Equals(name, StringComparison.InvariantCultureIgnoreCase)) return values[i];
                }
                throw new KeyNotFoundException();
            }
            #endregion

            #region IDynamicMetaObjectProvider

            DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
            {
                return new DataExpandoMetaObject(parameter, this);
            }

            class DataExpandoMetaObject : DynamicMetaObject
            {
                public DataExpandoMetaObject(Expression expression, DataExpando value) : base(expression, BindingRestrictions.Empty, value)
                {
                }

                public override IEnumerable<string> GetDynamicMemberNames()
                {
                    return ((DataExpando)Value).keys;
                }

                public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
                {
                    var expression = Expression.Call(
                                        Expression.Convert(Expression, LimitType),
                                        typeof(DataExpando).GetMethod(nameof(DataExpando.GetValue)),
                                        Expression.Constant(binder.Name)
                                    );
                    return new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
                }
            }
            #endregion
        }
        #endregion
    }
}
