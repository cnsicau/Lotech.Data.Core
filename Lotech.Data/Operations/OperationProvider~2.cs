using Lotech.Data.Descriptors;
using System;
using System.Data.Common;

namespace Lotech.Data.Operations
{
    /// <summary>
    /// 带结果返回执行
    /// </summary>
    /// <typeparam name="TOperationArg">操作参数</typeparam>
    /// <typeparam name="TResult"></typeparam>
    public abstract class OperationProvider<TOperationArg, TResult>
        : BuildableOperationProvider<Func<IDatabase, DbCommand, TOperationArg, TResult>, Func<IDatabase, TOperationArg, TResult>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        protected OperationProvider(IOperationBuilder<Func<IDatabase, DbCommand, TOperationArg, TResult>> builder) : base(builder)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        protected override Func<IDatabase, TOperationArg, TResult> OnCreate(EntityDescriptor descriptor)
        {
            var createCommand = Builder.BuildCommandProvider(descriptor);
            var invoker = Builder.BuildInvoker(descriptor);

            return (db, arg) =>
            {
                if (db == null) throw new ArgumentNullException(nameof(db));
                if (arg == null) throw new ArgumentNullException(nameof(arg));

                using (var command = createCommand(db))
                {
                    return invoker(db, command, arg);
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TBuilder"></typeparam>
        public class Instance<TBuilder> : OperationProvider<TOperationArg, TResult>
            where TBuilder : IOperationBuilder<Func<IDatabase, DbCommand, TOperationArg, TResult>>, new()
        {
            /// <summary>
            /// 
            /// </summary>
            public Instance() : base(new TBuilder()) { }
        }
    }
}
