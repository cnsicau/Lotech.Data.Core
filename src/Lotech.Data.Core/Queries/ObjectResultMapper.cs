using Lotech.Data.Queries.Dynamic;
using System.Data;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 动态对象结果映射
    /// </summary>
    public class ObjectResultMapper : IResultMapper<object>
    {
        object IResultMapper<object>.Map(IDataRecord record, object tearState)
        {
            var values = new object[((string[])tearState).Length];
            record.GetValues(values);
            return new DynamicEntity((string[])tearState, values);
        }

        void IResultMapper<object>.TearDown(object tearState) { }

        object IResultMapper<object>.TearUp(IDatabase database, IDataRecord record)
        {
            var fields = new string[record.FieldCount];
            for (int i = fields.Length - 1; i >= 0; i--)
            {
                fields[i] = record.GetName(i);
            }
            return fields;
        }
    }
}
