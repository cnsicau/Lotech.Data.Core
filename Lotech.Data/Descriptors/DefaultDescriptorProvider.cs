using System;
using System.Collections.Generic;
using System.Linq;

namespace Lotech.Data.Descriptors
{
    /// <summary>
    /// 默认描述符提供者
    ///     实现 基于反射+ Attribute的描述
    /// </summary>
    public class DefaultDescriptorProvider : IDescriptorProvider
    {
        /// <summary>
        /// 全局实例
        /// </summary>
        public static readonly IDescriptorProvider Instance = new DefaultDescriptorProvider();

        class AttributeEntityDescriptor<TEntity> where TEntity : class
        {
            internal static readonly IEntityDescriptor Instance;

            static AttributeEntityDescriptor()
            {
                var descriptor = new ReflectionEntityDescriptor<TEntity>();

                ApplyEntityAttribute(descriptor);
                ApplyMemberAttributes(descriptor);

                Instance = descriptor;
            }

            static void ApplyEntityAttribute(ReflectionEntityDescriptor<TEntity> descriptor)
            {
                var table = Attribute.GetCustomAttribute(typeof(TEntity), typeof(EntityAttribute)) as EntityAttribute;

                if (table != null)
                {
                    descriptor.Name = table.Name;
                    descriptor.Schema = table.Schema;
                }
            }

            static void ApplyMemberAttributes(ReflectionEntityDescriptor<TEntity> descriptor)
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

            static ICollection<KeyValuePair<ColumnAttribute, MemberDescriptor>> GetMemberAttributes(MemberDescriptor[] members)
            {
                return members
                    .Select(_ => new KeyValuePair<ColumnAttribute, MemberDescriptor>(
                        Attribute.GetCustomAttribute(_.Member, typeof(ColumnAttribute)) as ColumnAttribute,
                         _))
                    .Where(_ => _.Key != null).ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IEntityDescriptor GetEntityDescriptor<TEntity>(Operation operation) where TEntity : class
        {
            Console.WriteLine($"build descriptor for {operation} {typeof(TEntity).Name}");
            return AttributeEntityDescriptor<TEntity>.Instance;
        }
    }
}
