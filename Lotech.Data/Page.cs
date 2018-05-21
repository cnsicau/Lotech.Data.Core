namespace Lotech.Data
{
    /// <summary>
    /// 分页参数
    /// </summary>
    public class Page
    {
        /// <summary>
        /// 当前页号(0开始)
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 分页大小
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// 排序项
        /// </summary>
        public PageOrder[] Orders { get; set; }
    }
}
