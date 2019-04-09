using System;
using System.Data.Common;

namespace Lotech.Data
{
    /// <summary>
    /// DbProvider 实现
    /// </summary>
    public abstract class DbProviderDatabase : DbDatabase, IDatabase
    {
        private readonly DbProviderFactory dbProviderFactory;

        /// <summary>
        /// 获取 DbProviderFactory 实例
        /// </summary>
        public DbProviderFactory DbProviderFactory { get { return dbProviderFactory; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbProviderFactory"></param>
        /// <param name="services"></param>
        protected DbProviderDatabase(DbProviderFactory dbProviderFactory, IEntityServices services)
            : base(services)
        {
            if (dbProviderFactory == null)
                throw new ArgumentNullException(nameof(dbProviderFactory));

            this.dbProviderFactory = dbProviderFactory;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DbConnection CreateConnection()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new InvalidProgramException("ConnectionString is empty");
            }

            var connection = dbProviderFactory.CreateConnection();
            connection.ConnectionString = ConnectionString;
            return connection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DbCommand CreateCommand()
        {
            return dbProviderFactory.CreateCommand();
        }
    }
}