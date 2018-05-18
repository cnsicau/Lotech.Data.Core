using System;

namespace Lotech.Data.Descriptors
{
    /// <summary>
    /// 描述符提供
    /// </summary>
    public interface IDescriptorProvider
    {
        /// <summary>
        /// 获取指定类型的描述符
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IEntityDescriptor GetEntityDescriptor<TEntity>() where TEntity : class;
    }
}
