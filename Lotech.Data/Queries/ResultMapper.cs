using System;
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
        public virtual IDatabase Database { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected virtual IResultSource Source { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public abstract bool MapNext(out TValue result);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public virtual void TearUp(IResultSource source) { Source = source; }

        /// <summary>
        /// 
        /// </summary>
        public virtual void TearDown() { }

        #region Static Members
        readonly static Func<IResultMapper<TValue>> build;

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
