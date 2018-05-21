﻿using System;
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
    public abstract class QueryResult<TEntity> : IEnumerable<TEntity>
    {
        private readonly IResultMapper<TEntity> _mapper;
        private readonly Action<string> _log;

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
                _source.Dispose();
                _disposing?.Invoke(_count);
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
        /// <param name="mapper">结果映射器</param>
        /// <param name="log"></param>
        protected QueryResult(IResultMapper<TEntity> mapper, Action<string> log)
        {
            if (mapper == null)
                throw new ArgumentNullException("mapper");

            _mapper = mapper;
            _log = log;
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="log"></param>
        protected void Log(string log)
        {
            _log?.Invoke(log);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected IEnumerator<TEntity> CreateEnumerator(IDbCommand command)
        {
            var sw = Stopwatch.StartNew();
            var connection = command.Connection;
            if (connection == null)
                throw new InvalidOperationException("command.Connection is null.");
            var connectionState = connection.State;
            if (connectionState != ConnectionState.Open)
            {
                connection.Open();
                Log($"  Open connection at {DateTime.Now}. Elpased times: {sw.Elapsed}.");
                sw.Restart();
            }
            var reader = command.ExecuteReader();
            {
                IResultSource source = new DataReaderResultSource(reader);
                try
                {
                    return new QueryResultEnumerator(source, _mapper, (count) =>
                    {
                        sw.Stop();
                        Log($"  Complete enumerate {count} records. Elpased times: {sw.Elapsed}.");
                        if (connectionState != ConnectionState.Open && connection.State == ConnectionState.Closed)
                            Log($"  Close connection at {DateTime.Now}.");
                    });
                }
                catch
                {
                    _mapper.TearDown();
                    source.Dispose();
                    sw.Stop();
                    throw;
                }
                finally
                {
                    Log("  Elapsed times: " + sw.Elapsed);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator<TEntity> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() { return GetEnumerator(); }
    }
}
