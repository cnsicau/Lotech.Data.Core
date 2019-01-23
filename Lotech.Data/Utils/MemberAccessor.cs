using Lotech.Data.Descriptors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
                : typeof(void);
        }

        /// <summary>
        /// 是否静态成员
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static bool IsStaticMember(MemberInfo member)
        {
            return member.MemberType == MemberTypes.Property
                ? ((PropertyInfo)member).GetGetMethod().IsStatic
                : member.MemberType == MemberTypes.Field
                ? ((FieldInfo)member).IsStatic
                : member.MemberType == MemberTypes.Method
                ? ((MethodInfo)member).IsStatic
                : member.MemberType == MemberTypes.Constructor
                ? ((ConstructorInfo)member).IsStatic
                : false;
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

        /// <summary>
        /// 获取将 source.Member 赋值给 target.Member 方法
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="member"></param>
        /// <returns>(source, target) => target.Member = source.Member; </returns>
        static public Action<TEntity, TEntity> GetAssign<TEntity>(MemberInfo member) where TEntity : class
        {
            return AssignContainer<TEntity>.GetAssign(member);
        }

        /// <summary>
        /// 创建成员赋值方式
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="members"></param>
        /// <returns>(source, target) => { target.Member1 = source.Member1; target.Member1 = source.Member1; } </returns>
        static public Action<TEntity, TEntity> CreateAssign<TEntity>(IEnumerable<MemberInfo> members) where TEntity : class
        {
            var entity = Expression.Parameter(typeof(TEntity), "entity");
            var value = Expression.Parameter(typeof(TEntity), "value");

            return Expression.Lambda<Action<TEntity, TEntity>>(
                    Expression.Block(members.Select(_ => Expression.Assign(
                            Expression.MakeMemberAccess(entity, _),
                                Expression.MakeMemberAccess(value, _))
                        ))
                    , entity, value
                ).Compile();
        }

        static readonly Dictionary<int, Type> tupleBaseTypes = new Dictionary<int, Type>
        {
            {1, typeof(Tuple<>) },{2, typeof(Tuple<,>) },{3, typeof(Tuple<,,>) },{4, typeof(Tuple<,,,>) },
            {5, typeof(Tuple<,,,,>) },{6, typeof(Tuple<,,,,,>) },{7, typeof(Tuple<,,,,,,>) }
        };

        /// <summary>
        /// 创建 Hash键方法
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="members"></param>
        /// <returns></returns>
        static public Func<TEntity, System.Collections.IStructuralEquatable> CreateHashKey<TEntity>(IMemberDescriptor[] members) where TEntity : class
        {
            Type tupleType;
            Func<TEntity, System.Collections.IStructuralEquatable> r8 = null;
            var blocks = new List<Expression>();

            var entity = Expression.Parameter(typeof(TEntity));
            for (int i = 0; i < 7 && i < members.Length; i++)
            {
                blocks.Add(Expression.MakeMemberAccess(entity, members[i].Member));
            }

            if (members.Length <= 7)
            {
                tupleType = tupleBaseTypes[members.Length].MakeGenericType(members.Select(_ => _.Type).ToArray());
            }
            else
            {
                var types = new Type[8];
                for (int i = 0; i < 7; i++) types[i] = members[i].Type;

                var retains = new IMemberDescriptor[members.Length - 7];
                Array.Copy(members, 7, retains, 0, retains.Length);

                r8 = CreateHashKey<TEntity>(retains);
                blocks.Add(Expression.Call(r8.Method, entity));
                types[7] = typeof(System.Collections.IStructuralEquatable);

                tupleType = typeof(Tuple<,,,,,,,>).MakeGenericType(types);
            }

            return Expression.Lambda<Func<TEntity, System.Collections.IStructuralEquatable>>(
                    Expression.New(tupleType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Single(), blocks.ToArray())
                , entity
            ).Compile();
        }

        class AssignContainer<TEntity> where TEntity : class
        {
            static readonly ConcurrentDictionary<MemberInfo, Action<TEntity, TEntity>> assigns = new ConcurrentDictionary<MemberInfo, Action<TEntity, TEntity>>();

            internal static Action<TEntity, TEntity> GetAssign(MemberInfo member)
            {
                return assigns.GetOrAdd(member, key =>
                {
                    var source = Expression.Parameter(typeof(TEntity));
                    var target = Expression.Parameter(typeof(TEntity));

                    return Expression.Lambda<Action<TEntity, TEntity>>(
                            Expression.Assign(
                                    Expression.MakeMemberAccess(target, member),
                                    Expression.MakeMemberAccess(source, member)
                                )
                        , source, target).Compile();
                });
            }

        }
    }
}
