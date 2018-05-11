using Lotech.Data.Descriptors;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Transactions;

namespace Lotech.Data.Operations
{
    /// <summary>
    /// 自带事务
    /// </summary>
    /// <typeparam name="TOperationArg">操作参数</typeparam>
    /// <typeparam name="TResultElement">结果元素项</typeparam>
    public abstract class TransactionalOperationProvider<TOperationArg, TResultElement>
        : BuildableOperationProvider<Func<IDatabase, DbCommand, TOperationArg, TResultElement>, Func<IDatabase, IEnumerable<TOperationArg>, IEnumerable<TResultElement>>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        protected TransactionalOperationProvider(IOperationBuilder<Func<IDatabase, DbCommand, TOperationArg, TResultElement>> builder) : base(builder)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        protected override Func<IDatabase, IEnumerable<TOperationArg>, IEnumerable<TResultElement>> OnCreate(EntityDescriptor descriptor)
        {
            var createCommand = Builder.BuildCommandProvider(descriptor);
            var invoker = Builder.BuildInvoker(descriptor);

            return (db, args) =>
            {
                if (db == null) throw new ArgumentNullException(nameof(db));
                if (args == null) throw new ArgumentNullException(nameof(args));

                using (var enumerator = args.GetEnumerator())
                {
                    if (!enumerator.MoveNext()) return new TResultElement[0];

                    using (var command = createCommand(db))
                    {
                        if (TransactionManager.Current != null) // 已有事务时不重复开启
                        {
                            return LoopInvoke(db, command, enumerator, invoker);
                        }
                        else    // 开启事务
                        {
                            using (var transactionManager = new TransactionManager())
                            {
                                var result = LoopInvoke(db, command, enumerator, invoker);
                                transactionManager.Commit();
                                return result;
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
        /// <returns></returns>
        static IEnumerable<TResultElement> LoopInvoke(IDatabase db, DbCommand command, IEnumerator<TOperationArg> enumerator, Func<IDatabase, DbCommand, TOperationArg, TResultElement> invoker)
        {
            do
            {
                command.Parameters.Clear(); // 确保每次的参数成功重新绑定
                yield return invoker(db, command, enumerator.Current);
            }
            while (enumerator.MoveNext());
        }

        /// <summary>
        /// 
        /// </summary>
        public class Instance<TBuilder> : TransactionalOperationProvider<TOperationArg, TResultElement>
           where TBuilder : IOperationBuilder<Func<IDatabase, DbCommand, TOperationArg, TResultElement>>, new()
        {
            /// <summary>
            /// 
            /// </summary>
            public Instance() : base(new TBuilder()) { }
        }
    }
}
