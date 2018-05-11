using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Lotech.Data.Utils
{
    /// <summary>
    /// 
    /// </summary>
    static public class MemberAccessor
    {
        static readonly ConcurrentDictionary<MemberInfo, Func<object, object>> getters = new ConcurrentDictionary<MemberInfo, Func<object, object>>();
        static readonly ConcurrentDictionary<MemberInfo, Action<object, object>> setters = new ConcurrentDictionary<MemberInfo, Action<object, object>>();

        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static Type GetMemberValueType(MemberInfo member)
        {
            return member.MemberType == MemberTypes.Property
                ? ((PropertyInfo)member).PropertyType
                : member.MemberType == MemberTypes.Field
                ? ((FieldInfo)member).FieldType
                : throw new NotSupportedException("仅支持实例的字段、属性判定.");
        }

        /// <summary>
        /// 是否静态成员
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static bool IsStaticMember(MemberInfo member)
        {
            return member.MemberType == MemberTypes.Property
                ? ((PropertyInfo)member).GetMethod.IsStatic
                : member.MemberType == MemberTypes.Field
                ? ((FieldInfo)member).IsStatic
                : throw new NotSupportedException("仅支持实例的字段、属性判定.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        static public Func<object, object> GetGetter(MemberInfo member)
        {
            return getters.GetOrAdd(member, key =>
            {
                var valueType = GetMemberValueType(key);
                var entity = Expression.Parameter(typeof(object), "_");
                var isStatic = IsStaticMember(key);

                if (valueType == typeof(object))
                    return Expression.Lambda<Func<object, object>>(
                            Expression.MakeMemberAccess(
                                isStatic ? null : Expression.Convert(entity, key.DeclaringType), key)
                        , entity).Compile();

                return Expression.Lambda<Func<object, object>>(
                                Expression.Convert(
                                    Expression.MakeMemberAccess(
                                            isStatic ? null : Expression.Convert(entity, key.DeclaringType), key),
                                    typeof(object)), entity).Compile();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        static public Action<object, object> GetSetter(MemberInfo member)
        {
            return setters.GetOrAdd(member, key =>
            {
                var valueType = GetMemberValueType(key);

                var entity = Expression.Parameter(typeof(object), "_");
                var value = Expression.Parameter(typeof(object), "val");
                var isStatic = IsStaticMember(key);

                if (valueType == typeof(object))
                    return Expression.Lambda<Action<object, object>>(
                            Expression.Assign(
                                Expression.MakeMemberAccess(
                                        isStatic ? null : Expression.Convert(entity, key.DeclaringType), key),
                                value)
                        , entity, value).Compile();

                var convert = ValueConverter.GetConvert(typeof(object), valueType);
                return Expression.Lambda<Action<object, object>>(
                        Expression.Assign(
                            Expression.MakeMemberAccess(
                                    isStatic ? null : Expression.Convert(entity, key.DeclaringType), key),
                            Expression.Convert(
                                    Expression.Call(
                                         Expression.Constant(convert.Target),
                                        convert.Method,
                                        Expression.Convert(value, typeof(object))
                                    )
                                , valueType)
                        )
                    , entity, value).Compile();
            });
        }
    }
}
