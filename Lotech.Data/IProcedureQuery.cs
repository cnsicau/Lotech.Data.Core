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

        /// <summary>
        /// 添加命名参数
        ///     如 new { UserId = userId, Account = account }
        /// 将追加存储过程 @UserId, @Account参数
        /// </summary>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IProcedureQuery AddParameters<TParameter>(TParameter parameters) where TParameter : class;
    }
}
