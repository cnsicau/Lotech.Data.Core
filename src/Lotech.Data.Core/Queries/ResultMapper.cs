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
        /// <param name="record"></param>
        /// <returns></returns>
        public abstract TValue Map(IDataRecord record);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="record"></param>
        public abstract void Initialize(IDatabase database, IDataRecord record);

        #region Static Members
        readonly static Func<IResultMapper<TValue>> NewMapper;

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

            NewMapper = Expression.Lambda<Func<IResultMapper<TValue>>>(
                    Expression.New(mapperType.GetConstructor(Type.EmptyTypes))
                ).Compile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IResultMapper<TValue> Create() { return NewMapper(); }

        #endregion
    }
}
