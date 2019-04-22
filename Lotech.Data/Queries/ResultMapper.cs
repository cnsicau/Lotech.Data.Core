using System;
using System.Data;
using System.Linq.Expressions;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public abstract class ResultMapper<TValue> : IResultMapper<TValue>
    {
        /// <summary>
        /// 
        /// </summary>
        protected IDataReader reader;

        /// <summary>
        /// 
        /// </summary>
        public virtual IDatabase Database { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IDataReader Reader
        {
            get { return reader; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public abstract bool MapNext(out TValue result);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        public virtual void TearUp(IDataReader reader) { this.reader = reader; }

        /// <summary>
        /// 
        /// </summary>
        public virtual void TearDown() { reader.Close(); }

        #region Static Members
        readonly static Func<IResultMapper<TValue>> build;

        /// <summary>
        /// 
        /// </summary>
        static readonly Func<IDataRecord, int, TValue> read = CompileReadValue();

        static ResultMapper()
        {
            Type mapperType;

            if (typeof(TValue) == typeof(object))
            {
                mapperType = typeof(ObjectResultMapper);
            }
            else if (typeof(TValue).Assembly == typeof(int).Assembly)
            {
                mapperType = typeof(SimpleResultMapper<>).MakeGenericType(typeof(TValue));
            }
            else
            {
                mapperType = typeof(EntityResultMapper<>).MakeGenericType(typeof(TValue));
            }

            build = Expression.Lambda<Func<IResultMapper<TValue>>>(
                    Expression.New(mapperType.GetConstructor(Type.EmptyTypes))
                ).Compile();
        }

        static Func<IDataRecord, int, TValue> CompileReadValue()
        {
            var valueType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
            var typeName = Type.GetTypeCode(valueType).ToString();

            #region convert

            var to = typeof(Convert).GetMethod("To" + typeName, new Type[] { typeof(object) });

            var val = Expression.Parameter(typeof(object));

            var convert = Expression.Lambda<Func<object, TValue>>(
                    Expression.Condition(Expression.ReferenceNotEqual(Expression.Constant(null), val),
                        to == null ? Expression.Convert(val, typeof(TValue))
                            : valueType == typeof(TValue) ? Expression.Call(to, val)
                            : (Expression)Expression.Convert(Expression.Call(to, val), typeof(TValue)),
                           Expression.Constant(default(TValue), typeof(TValue))
                        )
                    , val
                ).Compile();
            #endregion

            #region CompileGet
            var get = typeof(IDataRecord).GetMethod("Get" + typeName, new Type[] { typeof(int) });
            if (get != null)
            {
                var reader = Expression.Parameter(typeof(IDataRecord));
                var index = Expression.Parameter(typeof(int));

                var read = Expression.Lambda<Func<IDataRecord, int, TValue>>(
                            valueType == typeof(TValue) ? Expression.Call(reader, get, index)
                                : (Expression)Expression.Convert(
                                    Expression.Call(reader, get, index), typeof(TValue))
                    , reader, index
                ).Compile();

                if (valueType != typeof(TValue)) // nullable
                {
                    return (record, i) =>
                    {
                        if (record.IsDBNull(i)) { return default(TValue); }
                        try { return read(record, i); }
                        catch { return convert(record.GetValue(i)); }
                    };
                }

                return (record, i) =>
                {
                    try { return read(record, i); }
                    catch { return convert(record.GetValue(i)); }
                };
            }
            else
            {
                return (record, i) => convert(record.GetValue(i));
            }
            #endregion
        }

        /// <summary>
        /// 读取值
        /// </summary>
        /// <param name="record"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        protected static TValue ReadRecordValue(IDataRecord record, int i) { return read(record, i); }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static IResultMapper<TValue> Create(IDatabase database)
        {
            if (database == null) throw new ArgumentNullException("database");

            var mapper = build();
            mapper.Database = database;
            return mapper;
        }


        #endregion
    }
}
