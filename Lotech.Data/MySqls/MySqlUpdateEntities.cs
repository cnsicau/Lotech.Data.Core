using Lotech.Data.Operations.Common;

namespace Lotech.Data.MySqls
{
    class MySqlUpdateEntities<TEntity, TSet> : CommonUpdateEntities<TEntity, TSet> where TEntity : class where TSet : class
    {
        public MySqlUpdateEntities() : base(MySqlEntityServices.Quote, MySqlEntityServices.BuildParameter, _ => new MySqlExpressionVisitor<TEntity>(_, Descriptors.Operation.Update)) { }
    }
}
