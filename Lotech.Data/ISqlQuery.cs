using System.Collections.Generic;

namespace Lotech.Data
{
    /// <summary>
    /// 查询语句
    /// </summary>
    /// <example>
    ///     <![CDATA[
    ///         var query = db.SqlQuery("SELECT * FROM xxx WHERE 1 = 1")
    ///                         .Append(" AND Name LIKE {0} || '%'",  "admin")
    ///                         .Append(" AND Deleted = {0}", true);
    ///     ]]>
    /// </example>
    public interface ISqlQuery
    {
        /// <summary>
        /// 追加片断
        /// </summary>
        /// <param name="snippet">片断中可以使用 {0}, {1} 作为参数占位</param>
        /// <param name="args">片断中占位将被替换的参数值</param>
        /// <returns></returns>
        ISqlQuery Append(string snippet, params object[] args);

        /// <summary>
        /// 追加无参数片断
        /// </summary>
        /// <param name="snippet"></param>
        /// <returns></returns>
        ISqlQuery Append(string snippet);

        /// <summary>
        /// 设置或获取关联库
        /// </summary>
        IDatabase Database { get; set; }

        /// <summary>
        /// 连接当前查询片断
        /// </summary>
        /// <returns></returns>
        string GetSnippets();

        /// <summary>
        /// 提取参数清单
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<string, object>> GetParameters();
    }
}
