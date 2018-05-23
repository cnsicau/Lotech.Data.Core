using System;

namespace Lotech.Data
{
    /// <summary>
    /// 表达式参数
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Name,nq} = {Type.Name,nq}({Value})")]
    public class SqlQueryParameter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public SqlQueryParameter(string name, Type type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public SqlQueryParameter(string name, object value)
        {
            Name = name;
            Type = value?.GetType() ?? typeof(object);
            Value = value;
        }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// 参数值
        /// </summary>
        public object Value { get; }
    }
}
