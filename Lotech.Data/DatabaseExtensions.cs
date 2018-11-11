using Lotech.Data.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Lotech.Data
{
    static class DatabaseExtensions
    {

        /// <summary>
        /// 从 Reader 中读取实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="database"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        static public IEnumerable<TEntity> ReadEntities<TEntity>(this IDatabase database, IDataReader reader)
        {
            var mapper = DbProviderDatabase.ResultMapper<TEntity>.Create(database);

            mapper.TearUp(new DataReaderResultSource(reader));
            try
            {
                TEntity entity;
                while (reader.Read() && mapper.MapNext(out entity))
                {
                    yield return entity;
                }
            }
            finally
            {
                mapper.TearDown();
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="database"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        static public IEnumerable<TEntity> ReadEntities<TEntity>(this IDatabase database, DbCommand command)
        {
            using (var reader = database.ExecuteReader(command))
            {
                return database.ReadEntities<TEntity>(reader);
            }
        }


        /// <summary>
        /// 读取
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="database"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        static public IEnumerable<TEntity> ReadEntities<TEntity>(this IDatabase database, string query)
        {
            using (var command = database.GetSqlStringCommand(query))
            {
                return database.ReadEntities<TEntity>(command);
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
        static public IEnumerable<TEntity> ReadEntities<TEntity>(this IDatabase database, CommandType commandType, string text)
        {
            using (var command = database.GetCommand(commandType, text))
            {
                return database.ReadEntities<TEntity>(command);
            }
        }
    }
}
