using Lotech.Data.Descriptors;
using System.Collections.Concurrent;

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
        static readonly ConcurrentDictionary<IDescriptorProvider, TOperation> operations = new ConcurrentDictionary<IDescriptorProvider, TOperation>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        static public TOperation Instance(IDescriptorProvider provider)
        {
            return operations.GetOrAdd(provider, CreateInstance);
        }

        static TOperation CreateInstance(IDescriptorProvider provider)
        {
            var entityDescriptor = provider.GetEntityDescriptor<TEntity>();
            return new TOperationProvider().Create(entityDescriptor);
        }
    }
}
