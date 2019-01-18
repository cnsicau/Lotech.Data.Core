using Lotech.Data.Queries;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Lotech.Data
{
    /// <summary>
    /// 扩展
    /// </summary>
    static public class DatabaseExtensions
    {
        class EntityReader<TEntity> : IEntityReader<TEntity>
        {
            private readonly IEnumerator<TEntity> enumerator;

            public EntityReader(IEnumerator<TEntity> enumerator) { this.enumerator = enumerator; }

            public void Close() { enumerator.Dispose(); }

            public void Dispose() { enumerator.Dispose(); }

            public TEntity GetValue() { return enumerator.Current; }

            public bool Read() { return enumerator.MoveNext(); }
        }

        /// <summary>
        /// 从 Reader 中读取实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="database"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        static IEnumerable<TEntity> CreateEntityReader<TEntity>(IDatabase database, IDataReader reader)
        {
            using (reader)
            {
                var mapper = DbProviderDatabase.ResultMapper<TEntity>.Create(database);

                mapper.TearUp(new DataReaderResultSource(reader));
                try
                {
                    TEntity entity = default(TEntity);
                    while (mapper.MapNext(out entity))
                    {
                        yield return entity;
                    }
                }
                finally
                {
                    mapper.TearDown();
                }
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="database"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        static public IEntityReader<TEntity> ExecuteEntityReader<TEntity>(this IDatabase database, DbCommand command)
        {
            var reader = database.ExecuteReader(command);
            try { return new EntityReader<TEntity>(CreateEntityReader<TEntity>(database, reader).GetEnumerator()); }
            catch
            {
                try { reader.Dispose(); } catch { }
                throw;
            }
        }


        /// <summary>
        /// 读取
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="database"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        static public IEntityReader<TEntity> ExecuteEntityReader<TEntity>(this IDatabase database, string query)
        {
            using (var command = database.GetSqlStringCommand(query))
            {
                return database.ExecuteEntityReader<TEntity>(command);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="database"></param>
        /// <param name="commandType"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        static public IEntityReader<TEntity> ExecuteEntityReader<TEntity>(this IDatabase database, CommandType commandType, string text)
        {
            using (var command = database.GetCommand(commandType, text))
            {
                return database.ExecuteEntityReader<TEntity>(command);
            }
        }
    }
}
