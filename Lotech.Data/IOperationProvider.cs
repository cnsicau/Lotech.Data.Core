using Lotech.Data.Descriptors;

namespace Lotech.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TOperation"></typeparam>
    public interface IOperationProvider<TOperation>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        TOperation Create(EntityDescriptor descriptor);
    }
}
