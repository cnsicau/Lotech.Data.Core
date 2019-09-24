using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Lotech.Data.Queries
{
    class DynamicEntityMetaObject : DynamicMetaObject
    {
        static class EntityConverter<T> where T : class, new()
        {
            /// <summary>
            /// var result = new T();
            /// object val;
            ///     if (values.TryGetValue("A", out val) &amp;&amp; val != null) result.A = (AType) val;
            ///     if (values.TryGetValue("A", out val) &amp;&amp; val != null) result.A = (AType) val;
            ///     if (values.TryGetValue("A", out val) &amp;&amp; val != null) result.A = (AType) val;
            /// return result;
            /// </summary>
            static readonly Func<IDictionary<string, object>, T> convert;

            static EntityConverter()
            {
                var values = Expression.Parameter(typeof(IDictionary<string, object>), "values");
                var expressions = new List<Expression>();
                var result = Expression.Variable(typeof(T), "result");
                var val = Expression.Variable(typeof(object), "val");

                expressions.Add(Expression.Assign(result, Expression.New(typeof(T))));

                var tryGetValue = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.TryGetValue));

                var members = typeof(T).GetMembers();
                foreach (var member in members)
                {
                    Type memberValueType;
                    if (member.MemberType == MemberTypes.Field)
                    {
                        memberValueType = ((FieldInfo)member).FieldType;
                    }
                    else if (member.MemberType == MemberTypes.Property && ((PropertyInfo)member).CanWrite)
                    {
                        memberValueType = ((PropertyInfo)member).PropertyType;
                    }
                    else continue;

                    var valueType = Nullable.GetUnderlyingType(memberValueType) ?? memberValueType;
                    if (valueType.IsEnum) valueType = Enum.GetUnderlyingType(valueType);
                    var to = typeof(Convert).GetMethod("To" + valueType.Name, new[] { typeof(object) });

                    var valueExpression = to != null ? (Expression)Expression.Call(to, val)
                        : !valueType.IsValueType ? Expression.Convert(val, valueType)
                        : Expression.ConvertChecked(Expression.Call(
                                    typeof(Convert).GetMethod(nameof(System.Convert.ChangeType), new[] { typeof(object), typeof(Type) }),
                                    val,
                                    Expression.Constant(valueType)), valueType);

                    expressions.Add(Expression.IfThen(
                        Expression.And(
                                Expression.Call(values, tryGetValue, Expression.Constant(member.Name), val),
                                Expression.NotEqual(val, Expression.Constant(null, typeof(object)))
                            ),
                        Expression.Assign(
                                Expression.MakeMemberAccess(result, member),
                                 valueType == memberValueType ? valueExpression
                                        : Expression.ConvertChecked(valueExpression, memberValueType)
                            )
                    ));
                }
                expressions.Add(result);

                var convertExpression = Expression.Lambda<Func<IDictionary<string, object>, T>>(
                    Expression.Block(typeof(T), new[] { val, result }, expressions)
                    , values);
                convert = convertExpression.Compile();
            }

            static public T Convert(IDictionary<string, object> values)
            {
                return convert(values);
            }
        }
        public DynamicEntityMetaObject(Expression expression, object value)
            : base(expression, BindingRestrictions.Empty, value)
        {
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            throw new NotSupportedException();
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var value = Expression.Call(
                               Expression.Convert(Expression, LimitType),
                               LimitType.GetMethod(nameof(DynamicEntity.GetValue), new Type[] { typeof(string) }),
                               Expression.Constant(binder.Name)
                       );
            return new DynamicMetaObject(value, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            if (binder.ReturnType == typeof(IDictionary)
                || binder.ReturnType == typeof(IDictionary<string, object>))
            {
                return new DynamicMetaObject(Expression.Convert(Expression, binder.ReturnType)
                    , BindingRestrictions.GetTypeRestriction(Expression, LimitType));
            }
            else if (binder.ReturnType.IsClass && !binder.ReturnType.IsAbstract && !binder.ReturnType.IsArray && !binder.ReturnType.IsByRef)
            {
                var converterType = typeof(EntityConverter<>).MakeGenericType(binder.ReturnType);
                var convertMethod = converterType.GetMethod(nameof(EntityConverter<object>.Convert));

                return new DynamicMetaObject(
                    Expression.Call(convertMethod, Expression.Convert(Expression, typeof(IDictionary<string, object>)))
                    , BindingRestrictions.GetTypeRestriction(Expression, LimitType));
            }
            return base.BindConvert(binder);
        }
    }
}
