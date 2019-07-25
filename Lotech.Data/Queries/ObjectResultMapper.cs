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
            result = new DataExpando(columns, values);
            return true;
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
                get
                {
                    for (int i = 0; i < keys.Length; i++)
                    {
                        if (keys[i].Equals(key, StringComparison.InvariantCultureIgnoreCase)) return values[i];
                    }
                    throw new KeyNotFoundException();
                }
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
                    var value = ((IDictionary<string, object>)Value)[binder.Name];
                    var expression = Expression.Constant(value, binder.ReturnType);
                    return new DynamicMetaObject(expression, BindingRestrictions.GetInstanceRestriction(expression, value));
                }
            }
            #endregion
        }
        #endregion
    }
}
