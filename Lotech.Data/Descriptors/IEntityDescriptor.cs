using System;

namespace Lotech.Data.Descriptors
{
    /// <summary>
    /// 实体描述符
    /// </summary>
    public interface IEntityDescriptor
    {
        /// <summary>
        /// 架构
        /// </summary>
        string Schema { get; }

        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 实体类型
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// 键
        /// </summary>
        IMemberDescriptor[] Keys { get; }

        /// <summary>
        /// 成员集
        /// </summary>
        IMemberDescriptor[] Members { get; }
    }
}
