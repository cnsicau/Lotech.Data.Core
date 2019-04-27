using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Lotech.Data.Utils
{
    public static class Convert<T>
    {
        public static readonly Func<object, T> Func;

        static Convert()
        {
            var value = Expression.Parameter(typeof(object), "value");
            Expression expression = CreateFromExpression(value);

            Func = Expression.Lambda<Func<object, T>>(expression, value).Compile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Expression CreateFromExpression(Expression value)
        {
            var type = typeof(T);
            var nullableBaseType = Nullable.GetUnderlyingType(typeof(T));
            var enumBaseType = (nullableBaseType ?? type).IsEnum ? (nullableBaseType ?? type)?.GetEnumUnderlyingType() : null;

            var change = typeof(Convert).GetMethod(nameof(Convert.ChangeType), new[] { typeof(object), typeof(Type) });

            if (nullableBaseType != null)
            {
                // Enum?   => value is EnumType ? (EnumType)value : value == null ? null : (EnumType)Val.ToVal(value);
                if (enumBaseType != null)
                {
                    var to = typeof(Convert).GetMethod("To" + enumBaseType.Name, new[] { typeof(object) });
                    return Expression.Condition(Expression.TypeIs(value, enumBaseType),
                            Expression.Convert(
                                Expression.Convert(value, nullableBaseType), type),
                            Expression.Condition(Expression.Or(
                                        Expression.Equal(value, Expression.Constant(DBNull.Value)),
                                        Expression.Equal(value, Expression.Constant(null))
                                    )
                                    , Expression.Constant(null, type)
                                    , Expression.Convert(
                                        Expression.Convert(to != null
                                            ? Expression.Call(to, value)
                                            : Expression.Call(change, value, Expression.Constant(enumBaseType)), nullableBaseType), type)
                                )
                        );
                }
                // Val? => value is ValType ? (Val)value : value == null ? null :(Val?).ToVal(value);
                else
                {
                    var to = typeof(Convert).GetMethod("To" + nullableBaseType.Name, new[] { typeof(object) });
                    return Expression.Condition(Expression.TypeIs(value, nullableBaseType),
                               Expression.Convert(value, type),
                               Expression.Condition(Expression.Or(
                                            Expression.Equal(value, Expression.Constant(DBNull.Value)),
                                            Expression.Equal(value, Expression.Constant(null))
                                        )
                                       , Expression.Constant(null, type)
                                        , Expression.Convert(to != null
                                               ? Expression.Call(to, value)
                                               : Expression.Call(change, value, Expression.Constant(nullableBaseType)), type)
                                   )
                           );
                }
            }
            else
            {
                // Enum => value is EnumBaseType ? (Enum)val :(Enum)ToVal(value);
                if (enumBaseType != null)
                {
                    var to = typeof(Convert).GetMethod("To" + enumBaseType.Name, new[] { typeof(object) });
                    return Expression.Condition(Expression.TypeIs(value, enumBaseType),
                               Expression.Convert(value, type),
                               Expression.Condition(Expression.Or(
                                            Expression.Equal(value, Expression.Constant(DBNull.Value)),
                                            Expression.Equal(value, Expression.Constant(null))
                                        )
                                       , Expression.Constant(Enum.ToObject(type, 0), type)
                                        , Expression.Convert(to != null
                                               ? Expression.Call(to, value)
                                               : Expression.Call(change, value, Expression.Constant(enumBaseType)), type)
                                   )
                           );
                }
                // Val => value is ValType ? (Val)value : value == null ? null :(Val?).ToVal(value);
                else
                {
                    var to = typeof(Convert).GetMethod("To" + type.Name, new[] { typeof(object) });
                    var parse = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string) }, null);

                    return Expression.Condition(Expression.TypeIs(value, type),
                               Expression.Convert(value, type),
                               parse != null ?
                                   Expression.Condition(Expression.TypeIs(value, typeof(string)),
                                        Expression.Call(parse, Expression.TypeAs(value, typeof(string))),
                                        Expression.Condition(Expression.Or(
                                                    Expression.Equal(value, Expression.Constant(DBNull.Value)),
                                                    Expression.Equal(value, Expression.Constant(null))
                                                )
                                               , Expression.Constant(default(T), type)
                                                , to != null ? (Expression)Expression.Call(to, value)
                                                       : Expression.Convert(
                                                            Expression.Call(change, value, Expression.Constant(type)), type)
                                           )
                                        )
                                : Expression.Condition(Expression.Or(
                                                Expression.Equal(value, Expression.Constant(DBNull.Value)),
                                                Expression.Equal(value, Expression.Constant(null))
                                            )
                                           , Expression.Constant(default(T), type)
                                            , to != null ? (Expression)Expression.Call(to, value)
                                                   : Expression.Convert(
                                                        Expression.Call(change, value, Expression.Constant(type)), type)
                                       )
                           );
                }
            }
        }

        public static T From(object value) { return Func.Invoke(value); }
    }
}
