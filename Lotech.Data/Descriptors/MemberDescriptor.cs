using System;
using System.Data;
using System.Reflection;

namespace Lotech.Data.Descriptors
{
    /// <summary>
    /// 成员描述符（属性、字段）
    /// </summary>
    public class MemberDescriptor : ICloneable
    {
        /// <summary>
        /// 成员名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 成员类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Db中的类型
        /// </summary>
        public DbType DbType { get; set; }

        /// <summary>
        /// 由DB生成，自增长或计算属性
        /// </summary>
        public bool DbGenerated { get; set; }

        /// <summary>
        /// 获取成员
        /// </summary>
        public MemberInfo Member { get; set; }

        /// <summary>
        /// 是否为主键成员
        /// </summary>
        public bool PrimaryKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MemberDescriptor Clone()
        {
            return new MemberDescriptor
            {
                DbGenerated = DbGenerated,
                DbType = DbType,
                Name = Name,
                Type = Type,
                Member = Member
            };
        }

        /// <summary>
        /// 克隆副本
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone() { return Clone(); }
    }
}
