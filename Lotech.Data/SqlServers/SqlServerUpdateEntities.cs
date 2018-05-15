using Lotech.Data.Operations.Common;

namespace Lotech.Data.SqlServers
{
    class SqlServerUpdateEntities<TEntity, TSet> : CommonUpdateEntities<TEntity, TSet> where TEntity : class where TSet : class
    {
        public SqlServerUpdateEntities() : base(SqlServerDatabase.Quote, SqlServerDatabase.BuildParameter, _ => new SqlServerExpressionVisitor<TEntity>(_)) { }
    }
}
