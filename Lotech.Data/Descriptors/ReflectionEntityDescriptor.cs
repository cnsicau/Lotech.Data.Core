using Lotech.Data.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace Lotech.Data.Descriptors
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class ReflectionEntityDescriptor<TEntity> : EntityDescriptor, ICloneable where TEntity : class
    {
        static readonly EntityDescriptor prototype = new EntityDescriptor();

        /// <summary>
        /// 获取原型描述符
        /// </summary>
        public static IEntityDescriptor Prototype { get { return prototype; } }

        static ReflectionEntityDescriptor()
        {
            prototype.Type = typeof(TEntity);
            prototype.Name = typeof(TEntity).Name;
            prototype.Schema = null;
            prototype.Members = typeof(TEntity).GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(FilterMember)
                    .Select(_ => new ReflectionMemberDescriptor(_))
                    .ToArray();
            prototype.Keys = prototype.Members.Where(_ => _.PrimaryKey).ToArray();
        }

        /// <summary>
        /// 过滤有效成员
        ///     仅允许简单类型的 Field和Property
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        static bool FilterMember(MemberInfo member)
        {
            Type valueType;
            if (member.MemberType == MemberTypes.Field)
            {
                valueType = ((FieldInfo)member).FieldType;
            }
            else if (member.MemberType == MemberTypes.Property)
            {
                valueType = ((PropertyInfo)member).PropertyType;
            }
            else    //  忽略非 Field 或 Property
            {
                return false;
            }

            return DbTypeParser.Parse(valueType) != System.Data.DbType.Object;  // 忽略非简单类型成员
        }

        /// <summary>
        /// 
        /// </summary>
        public ReflectionEntityDescriptor() : base(prototype) { }
    }
}
