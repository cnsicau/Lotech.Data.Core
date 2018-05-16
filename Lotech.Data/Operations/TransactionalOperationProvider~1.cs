using Lotech.Data.Descriptors;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Lotech.Data.Operations
{
    /// <summary>
    /// 自带事务
    /// </summary>
    /// <typeparam name="TOperationArgElement">操作参数</typeparam>
    public abstract class TransactionalOperationProvider<TOperationArgElement>
        : BuildableOperationProvider<Action<IDatabase, DbCommand, TOperationArgElement>, Action<IDatabase, IEnumerable<TOperationArgElement>>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        protected TransactionalOperationProvider(IOperationBuilder<Action<IDatabase, DbCommand, TOperationArgElement>> builder) : base(builder)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        protected override Action<IDatabase, IEnumerable<TOperationArgElement>> OnCreate(EntityDescriptor descriptor)
        {
            var createCommand = Builder.BuildCommandProvider(descriptor);
            var invoker = Builder.BuildInvoker(descriptor);

            return (db, args) =>
            {
                if (db == null) throw new ArgumentNullException(nameof(db));
                if (args == null) throw new ArgumentNullException(nameof(args));

                using (var enumerator = args.GetEnumerator())
                {
                    if (!enumerator.MoveNext()) return;

                    using (var command = createCommand(db))
                    {
                        if (TransactionManager.Current != null) // 事务范围探测
                        {
                            LoopInvoke(db, command, enumerator, invoker);
                        }
                        else    // 开启事务
                        {
                            using (var transactionManager = new TransactionManager())
                            {
                                LoopInvoke(db, command, enumerator, invoker);

                                transactionManager.Commit();
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="command"></param>
        /// <param name="enumerator"></param>
        /// <param name="invoker"></param>
        static void LoopInvoke(IDatabase db, DbCommand command, IEnumerator<TOperationArgElement> enumerator, Action<IDatabase, DbCommand, TOperationArgElement> invoker)
        {
            do
            {
                command.Parameters.Clear(); // 确保每次的参数成功重新绑定
                invoker(db, command, enumerator.Current);
            }
            while (enumerator.MoveNext());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TBuilder"></typeparam>
        public class Instance<TBuilder> : TransactionalOperationProvider<TOperationArgElement>
            where TBuilder : IOperationBuilder<Action<IDatabase, DbCommand, TOperationArgElement>>, new()
        {
            /// <summary>
            /// 
            /// </summary>
            public Instance() : base(new TBuilder()) { }
        }
    }
}
