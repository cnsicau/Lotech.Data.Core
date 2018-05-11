using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Lotech.Data.Utils
{
    /// <summary>
    /// 成员访问器
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public static class MemberAccessor<TEntity, TValue> where TEntity : class
    {
        static readonly ConcurrentDictionary<MemberInfo, Func<TEntity, TValue>> getters = new ConcurrentDictionary<MemberInfo, Func<TEntity, TValue>>();
        static readonly ConcurrentDictionary<MemberInfo, Action<TEntity, TValue>> setters = new ConcurrentDictionary<MemberInfo, Action<TEntity, TValue>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        static public Func<TEntity, TValue> GetGetter(MemberInfo member)
        {
            return getters.GetOrAdd(member, key =>
            {
                var valueType = MemberAccessor.GetMemberValueType(key);
                var isStatic = MemberAccessor.IsStaticMember(key);

                var entity = Expression.Parameter(typeof(TEntity), "_");

                if (valueType == typeof(TValue))
                    return Expression.Lambda<Func<TEntity, TValue>>(
                            Expression.MakeMemberAccess(isStatic ? null : entity, key), entity).Compile();

                var convert = ValueConverter.GetConvert(valueType, typeof(TValue));
                return Expression.Lambda<Func<TEntity, TValue>>(
                        Expression.Convert(
                            Expression.Call(
                                Expression.Constant(convert.Target),
                                convert.Method,
                                Expression.Convert(
                                    Expression.MakeMemberAccess(isStatic ? null : entity, key),
                                    typeof(object)
                                )
                            ), typeof(TValue)), entity).Compile();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        static public Action<TEntity, TValue> GetSetter(MemberInfo member)
        {
            return setters.GetOrAdd(member, key =>
            {
                var valueType = MemberAccessor.GetMemberValueType(key);
                var isStatic = MemberAccessor.IsStaticMember(key);

                var entity = Expression.Parameter(typeof(TEntity), "_");
                var value = Expression.Parameter(typeof(TValue), "val");

                if (valueType == typeof(TValue))
                    return Expression.Lambda<Action<TEntity, TValue>>(
                            Expression.Assign(
                                Expression.MakeMemberAccess(isStatic ? null : entity, key),
                                value)
                        , entity, value).Compile();

                var convert = ValueConverter.GetConvert(typeof(TValue), valueType);
                return Expression.Lambda<Action<TEntity, TValue>>(
                        Expression.Assign(
                            Expression.MakeMemberAccess(isStatic ? null : entity, key),
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
