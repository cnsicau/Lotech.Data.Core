using Lotech.Data.Utils;
using System;
using System.Reflection;

namespace Lotech.Data.Descriptors
{
    /// <summary>
    /// 基于反射的成员描述子
    /// </summary>
    public class ReflectionMemberDescriptor : MemberDescriptor, ICloneable
    {
        ReflectionMemberDescriptor() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        public ReflectionMemberDescriptor(MemberInfo member)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));

            Type = member.MemberType == MemberTypes.Field
                ? ((FieldInfo)member).FieldType
                : member.MemberType == MemberTypes.Property
                ? ((PropertyInfo)member).PropertyType
                : throw new NotSupportedException("invalid member type :" + member.MemberType);
            Name = member.Name;
            DbType = DbTypeParser.Parse(Type);
            DbGenerated = false;
        }
    }
}
