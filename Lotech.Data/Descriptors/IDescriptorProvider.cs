
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
        /// <param name="operation">操作</param>
        /// <returns></returns>
        IEntityDescriptor GetEntityDescriptor<TEntity>(Operation operation) where TEntity : class;
    }
}
