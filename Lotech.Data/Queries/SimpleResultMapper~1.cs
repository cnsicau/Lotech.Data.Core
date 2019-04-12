using System;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 简单类型映射int\short\bool等
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SimpleResultMapper<T> : ResultMapper<T>
    {
        /// <summary>
        /// 映射下一项
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool MapNext(out T result)
        {
            if (reader.Read())
            {
                try
                {
                    result = ReadRecordValue(reader, 0);
                    return true;
                }
                catch (Exception e)
                {
                    var value = reader.GetValue(0);
                    throw new InvalidCastException($"列{reader.GetName(0)}的值“{value}”({value?.GetType()})无法转换为{typeof(T)}.", e);
                }
            }

            result = default(T);
            return false;
        }
    }
}
