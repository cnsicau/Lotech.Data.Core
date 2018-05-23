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
                    .Where(_ => _.MemberType == MemberTypes.Property || _.MemberType == MemberTypes.Field)
                    .Select(_ => new ReflectionMemberDescriptor(_))
                    .ToArray();
            prototype.Keys = prototype.Members.Where(_ => _.PrimaryKey).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        public ReflectionEntityDescriptor() : base(prototype) { }
    }
}
