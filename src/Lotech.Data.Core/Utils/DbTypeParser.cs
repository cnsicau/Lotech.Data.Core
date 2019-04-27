using System;
using System.Collections.Concurrent;
using System.Data;

namespace Lotech.Data.Utils
{
    /// <summary>
    /// DBType 解析工具
    /// </summary>
    public class DbTypeParser
    {
        static readonly ConcurrentDictionary<Type, DbType> mapping = new ConcurrentDictionary<Type, DbType>();

        static DbTypeParser()
        {
            mapping[typeof(bool)] = DbType.Boolean;
            mapping[typeof(byte)] = DbType.Byte;
            mapping[typeof(sbyte)] = DbType.SByte;
            mapping[typeof(char)] = DbType.String;
            mapping[typeof(ushort)] = DbType.UInt16;
            mapping[typeof(short)] = DbType.Int16;
            mapping[typeof(uint)] = DbType.UInt32;
            mapping[typeof(int)] = DbType.Int32;
            mapping[typeof(ulong)] = DbType.UInt64;
            mapping[typeof(long)] = DbType.Int64;
            mapping[typeof(float)] = DbType.Single;
            mapping[typeof(double)] = DbType.Double;
            mapping[typeof(decimal)] = DbType.Decimal;
            mapping[typeof(DateTime)] = DbType.DateTime;
            mapping[typeof(Guid)] = DbType.Guid;

            mapping[typeof(bool?)] = DbType.Boolean;
            mapping[typeof(byte?)] = DbType.Byte;
            mapping[typeof(sbyte?)] = DbType.Byte;
            mapping[typeof(char?)] = DbType.String;
            mapping[typeof(ushort?)] = DbType.UInt16;
            mapping[typeof(short?)] = DbType.Int16;
            mapping[typeof(uint?)] = DbType.UInt32;
            mapping[typeof(int?)] = DbType.Int32;
            mapping[typeof(ulong?)] = DbType.UInt64;
            mapping[typeof(long?)] = DbType.Int64;
            mapping[typeof(float?)] = DbType.Single;
            mapping[typeof(double?)] = DbType.Double;
            mapping[typeof(decimal?)] = DbType.Decimal;
            mapping[typeof(DateTime?)] = DbType.DateTime;
            mapping[typeof(Guid?)] = DbType.Guid;

            mapping[typeof(byte[])] = DbType.Binary;
            mapping[typeof(string)] = DbType.String;
        }

        /// <summary>
        /// 通过数据类型查询数据库类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public DbType Parse(Type type)
        {
            DbType dbType;
            if (mapping.TryGetValue(type, out dbType)) return dbType;
            Type orignal = type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }
            if (!type.IsEnum || !mapping.TryGetValue(Enum.GetUnderlyingType(type), out dbType))
            {
                dbType = DbType.String;
            }
            mapping.TryAdd(orignal, dbType);
            return dbType;
        }
    }
}
