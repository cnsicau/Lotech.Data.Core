using System;
using System.Data;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 简单类型映射int\short\bool等
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SimpleResultMapper<T> : ResultMapper<T>
    {
        public override void Initialize(IDatabase database, IDataRecord record) { }

        /// <summary>
        /// 映射下一项
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public override T Map(IDataRecord record)
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
