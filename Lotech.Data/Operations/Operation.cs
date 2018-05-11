using Lotech.Data.Descriptors;

namespace Lotech.Data.Operations
{
    /// <summary>
    /// 操作工厂
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TOperation"></typeparam>
    /// <typeparam name="TOperationProvider"></typeparam>
    static public class Operation<TEntity, TOperation, TOperationProvider>
        where TEntity : class
        where TOperationProvider : IOperationProvider<TOperation>, new()
    {
        /// <summary>
        /// 获取基于属性注释描述的操作实例
        /// </summary>
        static public readonly TOperation Instance = new TOperationProvider().Create(AttributeDescriptorFactory.Create<TEntity>());
    }
}
