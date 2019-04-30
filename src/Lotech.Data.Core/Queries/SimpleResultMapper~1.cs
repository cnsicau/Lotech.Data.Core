using System;
using System.Data;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 简单类型映射int\short\bool等
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SimpleResultMapper<T> : IResultMapper<T>
    {
        object IResultMapper<T>.TearUp(IDatabase database, IDataRecord record) { return null; }

        void IResultMapper<T>.TearDown(object tearState) { }

        T IResultMapper<T>.Map(IDataRecord record, object tearState)
        {
            var value = record.GetValue(0);
            try
            {
                return Utils.Convert<T>.From(value);
            }
            catch (Exception e)
            {
                throw new InvalidCastException($"列{record.GetName(0)}的值“{value}”({value?.GetType()})无法转换为{typeof(T)}.", e);
            }
        }
    }
}
