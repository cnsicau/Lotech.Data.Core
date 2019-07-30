using System;
using System.Data;
using System.Linq.Expressions;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 简单类型映射int\short\bool等
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SimpleResultMapper<T> : ResultMapper<T>
    {
        delegate T MapDelegate(IDataRecord record, ref object value);

        static readonly MapDelegate map;

        static SimpleResultMapper()
        {
            var record = Expression.Parameter(typeof(IDataRecord), "record");
            var value = Expression.Parameter(typeof(object).MakeByRefType(), "value");

            var resultType = typeof(T);
            var valueType = Nullable.GetUnderlyingType(resultType) ?? resultType;

            var assign = Expression.Assign(value, Expression.Call(record
                     , typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetValue))
                     , Expression.Constant(0)
                 ));

            var to = typeof(Convert).GetMethod("To" + valueType.Name, new[] { typeof(object) });

            var isDBNullExpression = Expression.Call(
                        record, typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull)), Expression.Constant(0));
            var valueExpression = to != null ? (Expression)Expression.Call(to, value)
                : !valueType.IsValueType ? Expression.Convert(value, valueType)
                : Expression.ConvertChecked(Expression.Call(
                            typeof(Convert).GetMethod(nameof(Convert.ChangeType), new[] { typeof(object), typeof(Type) }),
                            value,
                            Expression.Constant(valueType)), valueType);

            var returnExpression = Expression.Condition(
                        isDBNullExpression,
                        Expression.Constant(default(T), resultType),
                        resultType == valueType ? valueExpression : Expression.ConvertChecked(valueExpression, resultType)
                    );

            map = Expression.Lambda<MapDelegate>(Expression.Block(
                    assign,
                    returnExpression
                ), record, value).Compile();
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
                object value = null;
                try
                {
                    result = map(reader, ref value);
                    return true;
                }
                catch (Exception e)
                {
                    throw new InvalidCastException($"列{reader.GetName(0)}的值“{value}”({value?.GetType()})无法转换为{typeof(T)}.", e);
                }
            }

            result = default(T);
            return false;
        }
    }
}
