using System;
using System.Linq.Expressions;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 简单类型映射int\short\bool等
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SimpleResultMapper<T> : ResultMapper<T>
    {
        static readonly Func<object, T> cast;

        static SimpleResultMapper()
        {
            var valueType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            var typeName = Type.GetTypeCode(valueType).ToString();

            var convert = typeof(Convert).GetMethod("To" + typeName, new Type[] { typeof(object) });

            var val = Expression.Parameter(typeof(object));

            cast = Expression.Lambda<Func<object, T>>(
                    Expression.Condition(Expression.ReferenceNotEqual(Expression.Constant(null), val),
                        convert == null ? Expression.Convert(val, typeof(T))
                            : valueType == typeof(T) ? Expression.Call(convert, val)
                            : (Expression)Expression.Convert(Expression.Call(convert, val), typeof(T)),
                        Expression.New(typeof(T))
                        )
                    , val
                ).Compile();
        }

        /// <summary>
        /// 映射下一项
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool MapNext(out T result)
        {
            if (reader.Read())
            {
                var value = reader.GetValue(0);
                try
                {
                    result = cast(value);
                    return true;
                }
                catch (Exception e) { throw new InvalidCastException($"列{reader.GetName(0)}的值“{value}”({value?.GetType()})无法转换为{typeof(T)}.", e); }
            }

            result = default(T);
            return false;
        }
    }
}
