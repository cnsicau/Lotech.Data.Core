using Lotech.Data.Operations.Common;

namespace Lotech.Data.Generics
{
    class GenericUpdateEntities<TEntity, TSet> : CommonUpdateEntities<TEntity, TSet> where TEntity : class where TSet : class
    {
        public GenericUpdateEntities() : base(null, null, _ => new Operations.SqlExpressionVisitor<TEntity>(_)) { }
    }
}
