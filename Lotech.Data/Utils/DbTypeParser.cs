using System;
using System.Collections.Generic;
using System.Data;

namespace Lotech.Data.Utils
{
    /// <summary>
    /// DBType 解析工具
    /// </summary>
    public class DbTypeParser
    {
        private static readonly Dictionary<Type, DbType> mapping = new Dictionary<Type, DbType>();

        static DbTypeParser()
        {
            mapping.Add(typeof(bool), DbType.Boolean);
            mapping.Add(typeof(byte), DbType.Byte);
            mapping.Add(typeof(sbyte), DbType.Byte);
            mapping.Add(typeof(char), DbType.String);
            mapping.Add(typeof(ushort), DbType.Int16);
            mapping.Add(typeof(short), DbType.Int16);
            mapping.Add(typeof(uint), DbType.Int32);
            mapping.Add(typeof(int), DbType.Int32);
            mapping.Add(typeof(ulong), DbType.Int64);
            mapping.Add(typeof(long), DbType.Int64);
            mapping.Add(typeof(float), DbType.Single);
            mapping.Add(typeof(double), DbType.Double);
            mapping.Add(typeof(decimal), DbType.Decimal);
            mapping.Add(typeof(DateTime), DbType.DateTime);
            mapping.Add(typeof(Guid), DbType.Guid);

            mapping.Add(typeof(bool?), DbType.Boolean);
            mapping.Add(typeof(byte?), DbType.Byte);
            mapping.Add(typeof(sbyte?), DbType.Byte);
            mapping.Add(typeof(char?), DbType.String);
            mapping.Add(typeof(ushort?), DbType.Int16);
            mapping.Add(typeof(short?), DbType.Int16);
            mapping.Add(typeof(uint?), DbType.Int32);
            mapping.Add(typeof(int?), DbType.Int32);
            mapping.Add(typeof(ulong?), DbType.Int64);
            mapping.Add(typeof(long?), DbType.Int64);
            mapping.Add(typeof(float?), DbType.Single);
            mapping.Add(typeof(double?), DbType.Double);
            mapping.Add(typeof(decimal?), DbType.Decimal);
            mapping.Add(typeof(DateTime?), DbType.DateTime);
            mapping.Add(typeof(Guid?), DbType.Guid);

            mapping.Add(typeof(byte[]), DbType.Binary);
            mapping.Add(typeof(string), DbType.AnsiString);
        }

        /// <summary>
        /// 通过数据类型查询数据库类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public DbType Parse(Type type)
        {
            if (type.IsEnum)
                return DbType.Int32;
            if (type.IsGenericType && type.GetGenericArguments()[0].IsEnum)
                return DbType.Int32;

            return mapping[type];
        }
    }
}
