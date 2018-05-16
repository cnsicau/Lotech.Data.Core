using System.Collections.Generic;
using System.Text;

namespace Lotech.Data.Utils
{
    static class StringBuilderExtensions
    {
        static public StringBuilder AppendJoin<T>(this StringBuilder builder, string seperator, IEnumerable<T> values)
        {
            return builder.Append(string.Join(seperator, values));
        }
    }
}
