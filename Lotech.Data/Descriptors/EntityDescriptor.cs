using System;
using System.Linq;

namespace Lotech.Data.Descriptors
{
    /// <summary>
    /// 实体描述符
    /// </summary>
    public class EntityDescriptor : ICloneable
    {
        /// <summary>
        /// 架构
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 实体类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 键
        /// </summary>
        public MemberDescriptor[] Keys { get; set; }

        /// <summary>
        /// 成员集
        /// </summary>
        public MemberDescriptor[] Members { get; set; }

        /// <summary>
        /// 拷贝构造
        /// </summary>
        /// <param name="prototype"></param>
        protected EntityDescriptor(EntityDescriptor prototype)
        {
            Type = prototype.Type;
            Name = prototype.Name;
            Schema = prototype.Schema;
            Members = prototype.Members.Select(_ => ((ReflectionMemberDescriptor)_).Clone()).ToArray();
            Keys = Members.Where(_ => _.PrimaryKey).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        public EntityDescriptor() { }

        /// <summary>
        /// 克隆副本
        /// </summary>
        /// <returns></returns>
        public EntityDescriptor Clone() { return new EntityDescriptor(this); }


        object ICloneable.Clone() { return Clone(); }
    }
}
