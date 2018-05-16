using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class CommandQueryResult<TEntity> : QueryResult<TEntity>
    {
        private readonly DbCommand _command;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="mapper"></param>
        /// <param name="log"></param>
        public CommandQueryResult(DbCommand command, IResultMapper<TEntity> mapper, Action<string> log)
            : base(mapper, log)
        {
            if(command  == null) throw new ArgumentNullException(nameof(command));
            _command  = command ;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator<TEntity> GetEnumerator()
        {
            Log("Executing " + _command.CommandType + " Query: \n" + _command.CommandText);
            foreach (DbParameter parameter in _command.Parameters)
                Log(" -- " + parameter.ParameterName + " = " + parameter.Value + "\t" + (parameter.Value ?? DBNull.Value).GetType().Name);

            return CreateEnumerator(_command);
        }
    }
}
