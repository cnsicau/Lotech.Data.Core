using Lotech.Data.Descriptors;
using System;
using System.Data.Common;

namespace Lotech.Data.Operations
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TOperationArg">操作参数</typeparam>
    public abstract class OperationProvider<TOperationArg>
        : BuildableOperationProvider<Action<IDatabase, DbCommand, TOperationArg>, Action<IDatabase, TOperationArg>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        protected OperationProvider(IOperationBuilder<Action<IDatabase, DbCommand, TOperationArg>> builder) : base(builder)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        protected override Action<IDatabase, TOperationArg> OnCreate(EntityDescriptor descriptor)
        {
            var createCommand = Builder.BuildCommandProvider(descriptor);
            var invoker = Builder.BuildInvoker(descriptor);

            return (db, arg) =>
            {
                if (db == null) throw new ArgumentNullException(nameof(db));
                if (arg == null) throw new ArgumentNullException(nameof(arg));

                using (var command = createCommand(db))
                {
                    invoker(db, command, arg);
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TBuilder"></typeparam>
        public class Instance<TBuilder> : OperationProvider<TOperationArg> where TBuilder : IOperationBuilder<Action<IDatabase, DbCommand, TOperationArg>>, new()
        {
            /// <summary>
            /// 
            /// </summary>
            public Instance() : base(new TBuilder()) { }
        }
    }
}
