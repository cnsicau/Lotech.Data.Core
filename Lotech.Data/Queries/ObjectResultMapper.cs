using System;
using System.Collections.Generic;

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

        bool IResultMapper<object>.MapNext(out object result)
        {
            if (!source.Next())
            {
                result = null;
                return false;
            }

            result = new System.Dynamic.ExpandoObject();
            // 将所有结果放入动态扩展对象中
            for (int i = source.Columns.Count - 1; i >= 0; i--)
            {
                var columnValue = source[i];

                ((IDictionary<string, object>)result)[source.Columns[i]] = columnValue == DBNull.Value ? null : columnValue;
            }
            return true;
        }

        void IResultMapper<object>.TearDown() { }
    }
}
