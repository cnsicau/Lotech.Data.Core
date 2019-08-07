using System.Collections.Generic;
using System.Data;

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
    public interface ISqlQuery : IQuery
    {
        /// <summary>
        /// 追加片断
        /// </summary>
        /// <param name="snippet">片断中可以使用 {0}, {1} 作为参数占位</param>
        /// <param name="args">片断中占位将被替换的参数值</param>
        /// <returns></returns>
        ISqlQuery Append(string snippet, IList<object> args);

        /// <summary>
        /// 追加无参数片断
        /// </summary>
        /// <param name="snippet"></param>
        /// <returns></returns>
        ISqlQuery Append(string snippet);

        /// <summary>
        /// 追加无参数片断
        /// </summary>
        /// <param name="snippet"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        ISqlQuery AppendString(string snippet, int offset, int length);

        /// <summary>
        /// 追加原始片断与参数，不处理任何占位，直接通过参数绑定
        /// </summary>
        /// <param name="snippet">不使用{n}占位的SQL片断，如：UserAccount = @account</param>
        /// <param name="parameters">绑定原始片断中的参数，顺序应与原始片断参数位置一致</param>
        /// <returns></returns>
        ISqlQuery AppendRaw(string snippet, IEnumerable<SqlQueryParameter> parameters);

        /// <summary>
        /// 追加原始片断与参数，不处理任何占位，直接通过参数绑定
        /// </summary>
        /// <param name="snippet">不使用{n}占位的SQL片断，如：UserAccount = @account</param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="parameters">绑定原始片断中的参数，顺序应与原始片断参数位置一致</param>
        /// <returns></returns>
        ISqlQuery AppendRaw(string snippet, int offset, int length, IEnumerable<SqlQueryParameter> parameters);

        /// <summary>
        /// 连接当前查询片断
        /// </summary>
        /// <returns></returns>
        string GetSnippets();

        /// <summary>
        /// 产生连续参数名称
        /// </summary>
        /// <returns></returns>
        string NextParameterName();

        /// <summary>
        /// 提取参数清单
        /// </summary>
        /// <returns></returns>
        IEnumerable<SqlQueryParameter> GetParameters();
    }
}
