using System;
using System.Collections.Concurrent;

namespace Lotech.Data.Utils
{
    /// <summary>
    /// 实体转换
    /// </summary>
    public class ValueConverter
    {
        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal delegate object ConvertDelegate(object value);

        static readonly ConcurrentDictionary<Tuple<Type, Type>, ConvertDelegate> converts = new ConcurrentDictionary<Tuple<Type, Type>, ConvertDelegate>();

        /// <summary>
        /// 创建转换
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        internal static ConvertDelegate GetConvert(Type sourceType, Type targetType)
        {
            return converts.GetOrAdd(Tuple.Create(sourceType, targetType), CreateConvert);
        }

        /// <summary>
        /// 创建转换
        /// </summary>
        /// <returns></returns>
        static ConvertDelegate CreateConvert(Tuple<Type, Type> key)
        {
            Type sourceType = key.Item1, targetType = key.Item2;

            sourceType = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
            // if (Nullable.GetUnderlyingType(sourceType) != null)
            //     throw new NotSupportedException("sourceType不能为可空类型");

            var sourceNullableType = sourceType;
            if (sourceType.IsValueType)
                sourceNullableType = typeof(Nullable<>).MakeGenericType(sourceType);

            var targetRealType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (sourceNullableType == targetType || targetType == typeof(object)) // 类型一致，直接返回 int? string DateTime?等
            {
                return val => val;
            }
            else if (sourceType == targetRealType) // 源类型是目标的可空类型
            {
                var defaultValue = Activator.CreateInstance(sourceType);
                return val => val == null ? defaultValue : val;
            }
            else if (targetType == typeof(string)) // 字符串
            {
                return val => val?.ToString();
            }
            else if (targetRealType.IsEnum)
            {
                var defaultValue = Activator.CreateInstance(targetType);
                if (sourceType.IsValueType)
                {
                    return val => val == null ? defaultValue : Enum.ToObject(targetRealType, val);
                }
            }
            else
            {
                var defaultVal = Activator.CreateInstance(targetType);
                return val => val == null ? defaultVal : Convert.ChangeType(val, targetRealType);
            }

            return val => { throw new InvalidCastException(val + "无法转换为" + targetType.FullName); };
        }
    }
}
