using Lotech.Data.Operations;
using Lotech.Data.Operations.Common;
using System;
using System.Data.Common;

namespace Lotech.Data.Generics
{
    class DeleteOperationBuilder<TEntity> : CommonDeleteOperationBuilder<TEntity>, IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>
        where TEntity : class
    { }
}
