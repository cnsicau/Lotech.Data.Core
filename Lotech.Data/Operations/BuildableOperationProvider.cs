using Lotech.Data.Descriptors;

namespace Lotech.Data.Operations
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TInvoker"></typeparam>
    /// <typeparam name="TOperation"></typeparam>
    public abstract class BuildableOperationProvider<TInvoker, TOperation>
        : IOperationProvider<TOperation>
    {
        private readonly IOperationBuilder<TInvoker> _builder;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        protected BuildableOperationProvider(IOperationBuilder<TInvoker> builder)
        {
            _builder = builder;
        }

        /// <summary>
        /// 获取构建器
        /// </summary>
        protected IOperationBuilder<TInvoker> Builder { get { return _builder; } }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public TOperation Create(EntityDescriptor descriptor)
        {
            return OnCreate(descriptor);
        }

        /// <summary>
        /// 实现创建过程
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        protected abstract TOperation OnCreate(EntityDescriptor descriptor);
    }
}
