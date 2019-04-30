using Lotech.Data.Queries.Dynamic;
using System.Data;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 动态对象结果映射
    /// </summary>
    public class ObjectResultMapper : ResultMapper<object>
    {
        string[] fields;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        public override void Initialize(IDatabase database, IDataRecord record)
        {
            fields = new string[record.FieldCount];
            for (int i = fields.Length - 1; i >= 0; i--)
            {
                fields[i] = record.GetName(i);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public override dynamic Map(IDataRecord record)
        {
            var values = new object[fields.Length];
            record.GetValues(values);
            return new DynamicEntity(fields, values);
        }
    }
}
