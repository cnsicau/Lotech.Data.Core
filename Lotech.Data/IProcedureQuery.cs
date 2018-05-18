namespace Lotech.Data
{
    /// <summary>
    /// 存储过程查询
    /// </summary>
    public interface IProcedureQuery : IQuery
    {
        /// <summary>
        /// 获取或设置存储过程名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 顺序添加存储过程参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IProcedureQuery AddParameter(string name, object value);
    }
}
