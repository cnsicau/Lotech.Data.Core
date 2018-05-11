using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 简单类型映射int\short\bool等
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SimpleResultMapper<T> : IResultMapper<T>
    {
        static readonly Assembly SimpleTypeAssembly = typeof(bool).Assembly;
        private IResultSource source;
        private Type underlyingType;

        static internal bool IsSimpleType()
        {
            return typeof(T) != typeof(object) && typeof(T).Assembly == SimpleTypeAssembly;
        }

        public void TearUp(IResultSource source)
        {
            if (!IsSimpleType())
                throw new InvalidProgramException("仅支持简单类型映射，如 int, short, long, decimal等");
            underlyingType = Nullable.GetUnderlyingType(typeof(T));

            this.source = source;
        }

        /// <summary>
        /// 映射下一项
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool MapNext(out T result)
        {
            if (source.Next())
            {
                object value = source[0];
                try
                {
                    if (value == DBNull.Value || value == null)
                    {
                        if (underlyingType != null)
                        {
                            result = default(T);
                            return true;
                        }
                    }
                    result = (T)Convert.ChangeType(source[0], underlyingType ?? typeof(T));
                }
                catch (FormatException e)
                {
                    throw new InvalidCastException($"列{source.Columns[0]}的值“{value}”无法转换为{typeof(T)}.", e);
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidCastException($"列{source.Columns[0]}的值“{value}”无法转换为{typeof(T)}.", e);
                }
                return true;
            }

            result = default(T);
            return false;
        }

        /// <summary>
        /// 完成
        /// </summary>
        public void TearDown() { }
    }
}
