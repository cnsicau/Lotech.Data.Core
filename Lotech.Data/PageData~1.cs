namespace Lotech.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class PageData<TEntity>
    {
        /// <summary>
        /// 
        /// </summary>
        public PageData() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count">总记录数</param>
        public PageData(int count) { Count = count; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count">总记录数</param>
        /// <param name="data">页数据</param>
        public PageData(int count, TEntity[] data)
        {
            Count = count;
            Data = data;
        }

        /// <summary>
        /// 设置或获取总记录数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 设置或获取当前页数据
        /// </summary>
        public TEntity[] Data { get; set; }
    }
}
