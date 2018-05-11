using System;

namespace Lotech.Data.Utils
{
    /// <summary>
    /// 名称格式化器
    /// </summary>
    class NameFormatProvider : IFormatProvider, ICustomFormatter
    {
        internal static readonly IFormatProvider Instance = new NameFormatProvider();

        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
                return this;
            else
                return null;

        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!(arg is string)) return arg.ToString();

            switch (format)
            {
                // upper
                case "u":
                case "U": return ((string)arg).ToUpper();
                // lower
                case "l":
                case "L": return ((string)arg).ToLower();

                default: return ((string)arg);
            }
        }
    }
}
