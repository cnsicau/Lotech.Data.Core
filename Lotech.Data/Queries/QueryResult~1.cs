using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly IResultMapper<TEntity> _mapper;
        private readonly DbCommand _command;
        private IResultSource _source;
        private int _count;
        private TEntity _current;
        private Stopwatch _stopwatch;

        TEntity IEnumerator<TEntity>.Current => _current;

        object IEnumerator.Current => _current;

        /// <summary>
        /// 构造查询结果
        /// </summary>
        /// <param name="database">DB</param>
        /// <param name="command"></param>
        /// <param name="mapper">结果映射器</param>
        public QueryResult(DbCommand command, IResultMapper<TEntity> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            _mapper = mapper;
            _command = command;

            if (_source == null)
            {
                var reader = _mapper.Database.ExecuteReader(_command);
                _source = new DataReaderResultSource(reader);
                _mapper.TearUp(_source);
                if (_mapper.Database.Log != null)
                {
                    _stopwatch = Stopwatch.StartNew();
                }
            }
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
            if (_source != null)
            {
                _mapper.TearDown();
                _source.Dispose();
                _source = null;
            }
        }
    }
}
