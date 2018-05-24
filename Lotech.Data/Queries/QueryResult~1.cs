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
    public class QueryResult<TEntity> : IEnumerable<TEntity>
    {
        private readonly IDatabase _database;
        private readonly IResultMapper<TEntity> _mapper;
        private readonly DbCommand _command;

        /// <summary>
        /// 结果枚举器
        /// </summary>
        private class QueryResultEnumerator : IEnumerator<TEntity>
        {
            private readonly IResultMapper<TEntity> _mapper;
            private IResultSource _source;
            private readonly Action<int> _disposing;
            private TEntity _current;
            private int _count;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="source"></param>
            /// <param name="mapper"></param>
            /// <param name="disposing">释放回调</param>
            public QueryResultEnumerator(IResultSource source, IResultMapper<TEntity> mapper, Action<int> disposing)
            {
                mapper.TearUp(source);
                _disposing = disposing;
                _source = source;
                _mapper = mapper;
            }

            object IEnumerator.Current { get { return _current; } }

            TEntity IEnumerator<TEntity>.Current { get { return _current; } }

            void IDisposable.Dispose()
            {
                _current = default(TEntity);
                _mapper.TearDown();
                _disposing?.Invoke(_count);
                _source.Dispose();
                _source = null;
            }

            bool IEnumerator.MoveNext()
            {
                var success = _mapper.MapNext(out _current);
                if (success)
                    _count++;
                return success;
            }

            void IEnumerator.Reset()
            {
                _current = default(TEntity);
            }

            ~QueryResultEnumerator()
            {
                if (_source != null)
                {
                    _source.Dispose();
                    _disposing?.Invoke(_count);
                }
            }
        }

        /// <summary>
        /// 构造查询结果
        /// </summary>
        /// <param name="database">DB</param>
        /// <param name="command"></param>
        /// <param name="mapper">结果映射器</param>
        public QueryResult(IDatabase database, DbCommand command, IResultMapper<TEntity> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            _database = database;
            _mapper = mapper;
            _command = command;
        }

        IEnumerator<TEntity> Execute()
        {
            var sw = Stopwatch.StartNew();
            var reader = _database.ExecuteReader(_command);
            {
                IResultSource source = new DataReaderResultSource(reader);
                try
                {
                    return new QueryResultEnumerator(source, _mapper, (count) =>
                    {
                        sw.Stop();
                        _database.Log?.Invoke($"  Complete read {count} {typeof(TEntity).Name} records. Elpased times: {sw.Elapsed}.");
                    });
                }
                catch
                {
                    _mapper.TearDown();
                    source.Dispose();
                    sw.Stop();
                    throw;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() { return Execute(); }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() { return Execute(); }
    }
}
