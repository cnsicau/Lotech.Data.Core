using Lotech.Data.Descriptors;
using System;
using System.Data.Common;

namespace Lotech.Data.Operations
{
    /// <summary>
    /// 操作创建器
    /// </summary>
    /// <typeparam name="TInvoker"></typeparam>
    public interface IOperationBuilder<TInvoker>
    {
        /// <summary>
        /// 创建DbCommand提升器
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        Func<IDatabase, DbCommand> BuildCommandProvider(EntityDescriptor descriptor);

        /// <summary>
        /// 创建执行器
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        TInvoker BuildInvoker(EntityDescriptor descriptor);
    }
}
