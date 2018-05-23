using Lotech.Data.Descriptors;
using System;
using System.Collections.Concurrent;

namespace Lotech.Data.Operations
{
    using CacheTuple = Tuple<IDescriptorProvider, Operation>;
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
        static readonly ConcurrentDictionary<CacheTuple, TOperation> operations = new ConcurrentDictionary<CacheTuple, TOperation>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="operation">操作</param>
        /// <returns></returns>
        static public TOperation Instance(IDescriptorProvider provider, Operation operation)
        {
            return operations.GetOrAdd(new CacheTuple(provider, operation), CreateInstance);
        }

        static TOperation CreateInstance(CacheTuple tuple)
        {
            var entityDescriptor = tuple.Item1.GetEntityDescriptor<TEntity>(tuple.Item2);
            return new TOperationProvider().Create(entityDescriptor);
        }
    }
}
