namespace Lotech.Data.Queries
{
    /// <summary>
    /// 原始参数值（不采取参数绑定，而直接占位替换）
    /// </summary>
    public interface IRawParameterValue
    {
        /// <summary>
        /// 原始值
        /// </summary>
        object Value { get; }
    }
}
