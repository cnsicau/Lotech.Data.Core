using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 
    /// </summary>
    abstract class Query : IQuery
    {
        protected IDatabase database;

        /// <summary>
        /// 
        /// </summary>
        public IDatabase Database
        {
            get { return database; }
            set { database = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        protected Query(IDatabase database) { this.database = database; }

        public abstract DbCommand CreateCommand();


        public object ExecuteScalar()
        {
            using (var command = CreateCommand())
            {
                return Database.ExecuteScalar(command);
            }
        }

        public DataSet ExecuteDataSet()
        {
            using (var command = CreateCommand())
            {
                return Database.ExecuteDataSet(command);
            }
        }

        public dynamic[] ExecuteEntities()
        {
            using (var command = CreateCommand())
            {
                return Database.ExecuteEntities(command);
            }
        }

        public TEntity[] ExecuteEntities<TEntity>()
        {
            using (var command = CreateCommand())
            {
                return Database.ExecuteEntities<TEntity>(command);
            }
        }

        public dynamic ExecuteEntity()
        {
            using (var command = CreateCommand())
            {
                return Database.ExecuteEntity(command);
            }
        }

        public TEntity ExecuteEntity<TEntity>()
        {
            using (var command = CreateCommand())
            {
                return Database.ExecuteEntity<TEntity>(command);
            }
        }

        public int ExecuteNonQuery()
        {
            using (var command = CreateCommand())
            {
                return Database.ExecuteNonQuery(command);
            }
        }

        public IDataReader ExecuteReader()
        {

            var command = CreateCommand();
            try
            {
                return Database.ExecuteReader(command);
            }
            catch
            {
                command.Dispose();
                throw;
            }
        }

        public TScalar ExecuteScalar<TScalar>()
        {

            using (var command = CreateCommand())
            {
                return Database.ExecuteScalar<TScalar>(command);
            }
        }

        public IEnumerable<TEntity> ExecuteEnumerable<TEntity>()
        {

            var command = CreateCommand();
            try
            {
                return Database.ExecuteEntityReader<TEntity>(command);
            }
            catch
            {
                command.Dispose();
                throw;
            }
        }

    }
}
