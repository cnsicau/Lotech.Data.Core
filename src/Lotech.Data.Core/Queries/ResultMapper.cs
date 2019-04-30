using System;
using System.Data;
using System.Linq.Expressions;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public abstract class ResultMapper<TValue>
    {
        public readonly static IResultMapper<TValue> Instance;

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
            Instance = (IResultMapper<TValue>)Activator.CreateInstance(mapperType);
        }
    }
}
