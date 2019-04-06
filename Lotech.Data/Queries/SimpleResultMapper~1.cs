﻿using System;
using System.Reflection;

namespace Lotech.Data.Queries
{
    using ConvertDelegate = Lazy<Utils.ValueConverter.ConvertDelegate>;
    /// <summary>
    /// 简单类型映射int\short\bool等
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SimpleResultMapper<T> : IResultMapper<T>
    {
        static readonly Assembly SimpleTypeAssembly = typeof(bool).Assembly;
        private IResultSource source;
        private Type underlyingType;
        private ConvertDelegate convert;

        static internal bool IsSimpleType()
        {
            return typeof(T) != typeof(object) && typeof(T).Assembly == SimpleTypeAssembly;
        }
        IDatabase IResultMapper<T>.Database { get; set; }

        public void TearUp(IResultSource source)
        {
            if (!IsSimpleType())
                throw new InvalidProgramException("仅支持简单类型映射，如 int, short, long, decimal等");
            underlyingType = Nullable.GetUnderlyingType(typeof(T));

            this.source = source;

            convert = new ConvertDelegate(() => Utils.ValueConverter.GetConvert(this.source.GetColumnType(0), typeof(T)));
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
                var value = source.GetColumnValue(0);
                var convert = this.convert.Value;
                try
                {
                    result = (T)convert(value);
                }
                catch (Exception e)
                {
                    var typedConvert = Utils.ValueConverter.GetTypedConvert(typeof(T));
                    if (convert != typedConvert)
                    {
                        try
                        {
                            result = (T)typedConvert(value);
                            convert = typedConvert;
                            return true;
                        }
                        catch { }
                    }
                    throw new InvalidCastException($"列{source.GetColumnName(0)}的值“{value}”{value?.GetType()}无法转换为{typeof(T)}.", e);
                }

                return true;
            }

            result = default(T);
            return false;
        }
    }
}
