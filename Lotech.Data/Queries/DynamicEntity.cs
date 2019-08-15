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
        private readonly IDictionary<string, int> indexedColumns;
        private readonly object[] values;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexedColumns"></param>
        /// <param name="values"></param>
        public DynamicEntity(IDictionary<string, int> indexedColumns, object[] values)
        {
            this.indexedColumns = indexedColumns;
            this.values = values;
        }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, int> IndexedColumn { get { return indexedColumns; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object GetValue(int index)
        {
            var value = values[index];
            if (value is DBNull) return null;
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetOrdinal(string name)
        {
            int ordinal;
            return indexedColumns.TryGetValue(name, out ordinal) ? ordinal : -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetValue(string name)
        {
            int ordinal;
            if (!indexedColumns.TryGetValue(name, out ordinal)) throw new KeyNotFoundException("column name " + name);

            var value = values[ordinal];
            if (value is DBNull) return null;
            return value;
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DynamicEntityMetaObject(parameter, this);
        }

        #region IDictionary<string, object>
        object IDictionary<string, object>.this[string key]
        {
            get { return GetValue(key); }
            set
            {
                var index = GetOrdinal(key);
                if (index == -1) throw new KeyNotFoundException();
                values[index] = value;
            }
        }

        ICollection<string> IDictionary<string, object>.Keys => indexedColumns.Keys;

        ICollection<object> IDictionary<string, object>.Values => values;

        int ICollection<KeyValuePair<string, object>>.Count => indexedColumns.Count;

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => true;

        void IDictionary<string, object>.Add(string key, object value) { throw new ReadOnlyException(); }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) { throw new ReadOnlyException(); }

        void ICollection<KeyValuePair<string, object>>.Clear() { throw new ReadOnlyException(); }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            var ordinal = GetOrdinal(item.Key);
            return ordinal != -1 && GetValue(ordinal) == item.Value;
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return indexedColumns.ContainsKey(key);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var item in indexedColumns)
            {
                array[arrayIndex] = new KeyValuePair<string, object>(item.Key, GetValue(item.Value));
                if (++arrayIndex == array.Length) break;
            }
        }

        IEnumerable<KeyValuePair<string, object>> GetKeyValuePairs()
        {
            foreach (var item in indexedColumns)
            {
                yield return new KeyValuePair<string, object>(item.Key, GetValue(item.Value));
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
            int ordinal;
            if (!indexedColumns.TryGetValue(key, out ordinal))
            {
                value = null;
                return false;
            }

            value = values[ordinal];
            if (value is DBNull) value = null;
            return true;
        }
        #endregion
    }
}
