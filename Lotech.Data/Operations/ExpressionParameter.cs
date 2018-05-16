using System;

namespace Lotech.Data.Operations
{
    /// <summary>
    /// 表达式参数
    /// </summary>
    public class ExpressionParameter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public ExpressionParameter(string name, Type type, object value)
        {
            Name = name;
            Type = type;
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
