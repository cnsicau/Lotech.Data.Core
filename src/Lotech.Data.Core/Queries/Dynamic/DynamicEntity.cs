using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace Lotech.Data.Queries.Dynamic
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicEntity : IDynamicMetaObjectProvider
    {
        private readonly string[] fields;
        private readonly object[] values;

        public DynamicEntity(string[] fields, object[] values)
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
    }
}
