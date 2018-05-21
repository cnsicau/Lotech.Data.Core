namespace Lotech.Data
{
    /// <summary>
    ///  排序
    /// </summary>
    public class PageOrder
    {
        /// <summary>
        /// 排序列名
        /// </summary>
        public string Column { get; set; }

        /// <summary>
        /// 排序方向
        /// </summary>
        public PageOrderDirection Direction { get; set; }
    }
}
