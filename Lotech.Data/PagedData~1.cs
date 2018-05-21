namespace Lotech.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class PagedData<TEntity> where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        public PagedData() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count">总记录数</param>
        public PagedData(int count) { Count = count; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count">总记录数</param>
        /// <param name="data">页数据</param>
        public PagedData(int count, TEntity[] data)
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
