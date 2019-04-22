using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 查询结果封装
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class QueryResult<TEntity> : IEnumerable<TEntity>, IEnumerator<TEntity>
    {
        private IResultMapper<TEntity> _mapper;
        private int _count;
        private TEntity _current;
        private Stopwatch _stopwatch;

        TEntity IEnumerator<TEntity>.Current => _current;

        object IEnumerator.Current => _current;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="mapper"></param>
        public QueryResult(IDataReader reader, IResultMapper<TEntity> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            _mapper = mapper;
            _mapper.TearUp(reader);
            if (_mapper.Database.Log != null)
                _stopwatch = Stopwatch.StartNew();
        }

        IEnumerator IEnumerable.GetEnumerator() { return this; }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() { return this; }

        bool IEnumerator.MoveNext()
        {
            return _mapper.MapNext(out _current) && ++_count > 0;
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        void IDisposable.Dispose()
        {
            if (_mapper.Database.Log != null)
            {
                _mapper.Database.Log($"  Complete read {_count} {typeof(TEntity).Name} records. Elpased times: {_stopwatch.Elapsed}.");
                _stopwatch.Stop();
            }
            _mapper.TearDown();
        }
    }
}
