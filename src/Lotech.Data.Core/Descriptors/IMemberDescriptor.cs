using System;
using System.Data;
using System.Reflection;

namespace Lotech.Data.Descriptors
{
    /// <summary>
    /// 成员描述符
    /// </summary>
    public interface IMemberDescriptor
    {
        /// <summary>
        /// 成员名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 成员类型
        /// </summary>
        Type Type { get; set; }

        /// <summary>
        /// Db中的类型
        /// </summary>
        DbType DbType { get; set; }

        /// <summary>
        /// 由DB生成，自增长或计算属性
        /// </summary>
        bool DbGenerated { get; set; }

        /// <summary>
        /// 获取成员
        /// </summary>
        MemberInfo Member { get; set; }

        /// <summary>
        /// 是否为主键成员
        /// </summary>
        bool PrimaryKey { get; set; }
    }
}
