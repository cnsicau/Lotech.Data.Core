using Lotech.Data.Queries;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Lotech.Data
{
    /// <summary>
    /// ¿©’π
    /// </summary>
    static public class DatabaseExtensions
    {
        /// <summary>
        /// ∂¡»°
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="database"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        static public IEnumerable<TEntity> ExecuteEntityReader<TEntity>(this IDatabase database, DbCommand command)
        {
            return new QueryResult<TEntity>(command, ResultMapper<TEntity>.Create(database));
        }

        /// <summary>
        /// ∂¡»°
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="database"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        static public IEnumerable<TEntity> ExecuteEntityReader<TEntity>(this IDatabase database, string query)
        {
            return ExecuteEntityReader<TEntity>(database, CommandType.Text, query);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="database"></param>
        /// <param name="commandType"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        static public IEnumerable<TEntity> ExecuteEntityReader<TEntity>(this IDatabase database, CommandType commandType, string text)
        {
            var command = database.GetCommand(commandType, text);
            try { return new QueryResult<TEntity>(command, ResultMapper<TEntity>.Create(database)); }
            catch
            {
                command.Dispose();
                throw;
            }
        }
    }
}
