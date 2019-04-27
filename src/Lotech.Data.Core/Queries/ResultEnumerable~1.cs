using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 查询结果封装
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class ResultEnumerable<TEntity> : IEnumerable<TEntity>, IEnumerator<TEntity>
    {
        private IResultMapper<TEntity> _mapper;
        private IDataReader _reader;
        private int _count;
        private Stopwatch _stopwatch;
        private Action<string> _log;

        TEntity IEnumerator<TEntity>.Current => _mapper.Map(_reader);

        object IEnumerator.Current => _mapper.Map(_reader);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="reader"></param>
        /// <param name="mapper"></param>
        public ResultEnumerable(IDatabase database, IDataReader reader, IResultMapper<TEntity> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            _reader = reader;
            _mapper = mapper;
            _mapper.Initialize(database, reader);
            if (database.Log != null)
            {
                _log = database.Log;
                _stopwatch = Stopwatch.StartNew();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() { return this; }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() { return this; }

        bool IEnumerator.MoveNext()
        {
            if (_reader.Read())
            {
                _count++;
                return true;
            }
            return false;
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        void IDisposable.Dispose()
        {
            if (_log != null)
            {
                _log($"  Complete read {_count} {typeof(TEntity).Name} records. Elpased times: {_stopwatch.Elapsed}.");
                _stopwatch.Stop();
            }
            _reader.Close();
        }
    }
}
