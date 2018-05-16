using System;
using System.Linq;
using System.Collections.Generic;

namespace Lotech.Data.Descriptors
{
    /// <summary>
    /// 基于属性描述符工厂
    /// </summary>
    public class AttributeDescriptorFactory
    {
        static class AttributeEntityDescriptor<TEntity> where TEntity : class
        {
            static AttributeEntityDescriptor()
            {
                var descriptor = new ReflectionEntityDescriptor<TEntity>();

                ApplyEntityAttribute(descriptor);
                ApplyMemberAttributes(descriptor);

                Instance = descriptor;
            }

            static void ApplyEntityAttribute(EntityDescriptor descriptor)
            {
                var table = Attribute.GetCustomAttribute(typeof(TEntity), typeof(EntityAttribute)) as EntityAttribute;

                if (table != null)
                {
                    descriptor.Name = table.Name;
                    descriptor.Schema = table.Schema;
                }
            }

            static void ApplyMemberAttributes(EntityDescriptor descriptor)
            {
                var attrs = GetMemberAttributes(descriptor.Members);
                // 同步属性中设置项
                foreach (var item in attrs)
                {
                    var attr = item.Key;
                    var member = item.Value;
                    member.DbGenerated = attr.DbGenerated;
                    member.DbType = attr.DbType ?? member.DbType;
                    member.Name = attr.Name ?? member.Name;
                    member.PrimaryKey = attr.PrimaryKey;
                }
                // 同步键
                descriptor.Keys = attrs.Where(_ => _.Key.PrimaryKey).Select(_ => _.Value).ToArray();
            }

            static IDictionary<ColumnAttribute, MemberDescriptor> GetMemberAttributes(MemberDescriptor[] members)
            {
                return members
                    .Select(_ => new KeyValuePair<ColumnAttribute, MemberDescriptor>(
                        Attribute.GetCustomAttribute(_.Member, typeof(ColumnAttribute)) as ColumnAttribute,
                         _))
                    .Where(_ => _.Key != null)
                    .ToDictionary(_ => _.Key, _ => _.Value);
            }


            internal static readonly EntityDescriptor Instance;
        }

        /// <summary>
        /// 创建描述符
        /// </summary>
        /// <returns></returns>
        static public EntityDescriptor Create<TEntity>() where TEntity : class
        {
            return AttributeEntityDescriptor<TEntity>.Instance;
        }
    }
}
