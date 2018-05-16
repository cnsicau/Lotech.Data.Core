using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 动态对象结果映射
    /// </summary>
    public class ObjectResultMapper : IResultMapper<object>
    {
        private IResultSource source;

        void IResultMapper<object>.TearUp(IResultSource source)
        {
            this.source = source;
        }

        bool IResultMapper<object>.MapNext(out dynamic result)
        {
            if (!source.Next())
            {
                result = null;
                return false;
            }
            result = new DataExpando();

            // 将所有结果放入动态扩展对象中
            for (int i = source.Columns.Count - 1; i >= 0; i--)
            {
                var columnValue = source[i];
                ((IDictionary<string, object>)result)[source.Columns[i]] = columnValue == DBNull.Value ? null : columnValue;
            }
            return true;
        }

        void IResultMapper<object>.TearDown() { }

        #region DataExpando
        class DataExpando : DynamicObject, IDictionary<string, object>
        {
            private IDictionary<string, object> values = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                return values.TryGetValue(binder.Name, out result);
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                values[binder.Name] = value;
                return true;
            }

            public object this[string key]
            {
                get { return values[key]; }
                set { values[key] = value; }
            }

            public ICollection<string> Keys => values.Keys;

            public ICollection<object> Values => values.Values;

            public int Count => values.Count;

            public bool IsReadOnly => values.IsReadOnly;

            public void Add(string key, object value)
            {
                values.Add(key, value);
            }

            public void Add(KeyValuePair<string, object> item)
            {
                values.Add(item);
            }

            public void Clear()
            {
                values.Clear();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                return values.Contains(item);
            }

            public bool ContainsKey(string key)
            {
                return values.ContainsKey(key);
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                values.CopyTo(array, arrayIndex);
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                return values.GetEnumerator();
            }

            public bool Remove(string key)
            {
                return values.Remove(key);
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                return values.Remove(item);
            }

            public bool TryGetValue(string key, out object value)
            {
                return values.TryGetValue(key, out value);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return values.GetEnumerator();
            }
        }
        #endregion
    }
}
