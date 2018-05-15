using Lotech.Data.Operations.Common;

namespace Lotech.Data.SQLites
{
    class SQLiteUpdateEntities<TEntity, TSet> : CommonUpdateEntities<TEntity, TSet> where TEntity : class where TSet : class
    {
        public SQLiteUpdateEntities() : base(SQLiteDatabase.Quote, SQLiteDatabase.BuildParameter, _ => new SQLiteExpressionVisitor<TEntity>(_)) { }
    }
}
