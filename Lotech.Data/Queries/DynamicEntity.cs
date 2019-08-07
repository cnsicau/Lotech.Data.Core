using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq.Expressions;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 
    /// </summary>
    class DynamicEntity : IDictionary<string, object>, IDynamicMetaObjectProvider
    {
        private readonly string[] fields;
        private readonly object[] values;

        internal DynamicEntity(string[] fields, object[] values)
        {
            this.fields = fields;
            this.values = values;
        }

        public string[] Fields { get { return fields; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object GetValue(int index)
        {
            return values[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetOrdinal(string name)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (string.Compare(fields[i], name, true) == 0) return i;
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetValue(string name)
        {
            return GetValue(GetOrdinal(name));
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DynamicEntityMetaObject(parameter, this);
        }

        #region Dictionary

        #region IDictionary<string, object>
        object IDictionary<string, object>.this[string key]
        {
            get
            {
                var index = GetOrdinal(key);
                if (index == -1) throw new KeyNotFoundException();
                return GetValue(index);
            }
            set
            {
                var index = GetOrdinal(key);
                if (index == -1) throw new KeyNotFoundException();
                values[index] = value;
            }
        }

        ICollection<string> IDictionary<string, object>.Keys => fields;

        ICollection<object> IDictionary<string, object>.Values => values;

        int ICollection<KeyValuePair<string, object>>.Count => fields.Length;

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
            return GetOrdinal(key) != -1;
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                array[arrayIndex + i] = new KeyValuePair<string, object>(fields[i], values[i]);
            }
        }

        IEnumerable<KeyValuePair<string, object>> GetKeyValuePairs()
        {
            for (int i = 0; i < fields.Length; i++)
            {
                yield return new KeyValuePair<string, object>(fields[i], values[i]);
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
            var index = GetOrdinal(key);
            if (index != -1)
            {
                value = GetValue(index);
                return true;
            }
            value = null;
            return false;
        }
        #endregion
        #endregion
    }
}
