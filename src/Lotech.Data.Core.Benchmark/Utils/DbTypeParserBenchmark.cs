using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace Lotech.Data.Utils.Benchmark
{
    [LongRunJob, MemoryDiagnoser, InvocationCount(100000)]
    public class DbTypeParserBenchmark
    {
        private static readonly Dictionary<Type, DbType> dictionary = new Dictionary<Type, DbType>();
        private static readonly ConcurrentDictionary<Type, DbType> concurrencyDictionary;

        static DbTypeParserBenchmark()
        {
            dictionary.Add(typeof(bool), DbType.Boolean);
            dictionary.Add(typeof(byte), DbType.Byte);
            dictionary.Add(typeof(sbyte), DbType.Byte);
            dictionary.Add(typeof(char), DbType.String);
            dictionary.Add(typeof(ushort), DbType.UInt16);
            dictionary.Add(typeof(short), DbType.Int16);
            dictionary.Add(typeof(uint), DbType.Int32);
            dictionary.Add(typeof(int), DbType.Int32);
            dictionary.Add(typeof(ulong), DbType.Int64);
            dictionary.Add(typeof(long), DbType.Int64);
            dictionary.Add(typeof(float), DbType.Single);
            dictionary.Add(typeof(double), DbType.Double);
            dictionary.Add(typeof(decimal), DbType.Decimal);
            dictionary.Add(typeof(DateTime), DbType.DateTime);
            dictionary.Add(typeof(Guid), DbType.Guid);

            dictionary.Add(typeof(bool?), DbType.Boolean);
            dictionary.Add(typeof(byte?), DbType.Byte);
            dictionary.Add(typeof(sbyte?), DbType.Byte);
            dictionary.Add(typeof(char?), DbType.String);
            dictionary.Add(typeof(ushort?), DbType.Int16);
            dictionary.Add(typeof(short?), DbType.Int16);
            dictionary.Add(typeof(uint?), DbType.Int32);
            dictionary.Add(typeof(int?), DbType.Int32);
            dictionary.Add(typeof(ulong?), DbType.Int64);
            dictionary.Add(typeof(long?), DbType.Int64);
            dictionary.Add(typeof(float?), DbType.Single);
            dictionary.Add(typeof(double?), DbType.Double);
            dictionary.Add(typeof(decimal?), DbType.Decimal);
            dictionary.Add(typeof(DateTime?), DbType.DateTime);
            dictionary.Add(typeof(Guid?), DbType.Guid);

            dictionary.Add(typeof(byte[]), DbType.Binary);
            dictionary.Add(typeof(string), DbType.String);

            concurrencyDictionary = new ConcurrentDictionary<Type, DbType>(dictionary);
        }

        [Params(typeof(int), typeof(DbTypeParserBenchmark), typeof(long?), typeof(TypeCode), typeof(TypeCode?))]
        public Type Type { get; set; }

        [Benchmark]
        public DbType UseDictionary()
        {
            var type = Type;
            DbType dbType;
            if (dictionary.TryGetValue(type, out dbType)) return dbType;
            if (type.IsEnum
                || type.IsGenericType && (type = Nullable.GetUnderlyingType(type)) != null && type.IsEnum)
            {
                if (dictionary.TryGetValue(Enum.GetUnderlyingType(type), out dbType)) return dbType;
            }
            return DbType.String;
        }

        [Benchmark]
        public DbType UseConcurrencyDictionary()
        {
            return concurrencyDictionary.GetOrAdd(Type, type =>
            {
                DbType dbType;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = type.GetGenericArguments()[0];
                }
                if (type.IsEnum && concurrencyDictionary.TryGetValue(Enum.GetUnderlyingType(type), out dbType))
                {
                    return dbType;
                }
                return DbType.String;
            });
        }

        [Benchmark]
        public DbType Complete()
        {
            return DbTypeParser.Parse(Type);
        }
    }
}
