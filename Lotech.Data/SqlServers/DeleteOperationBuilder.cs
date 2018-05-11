using Lotech.Data.Operations;
using Lotech.Data.Operations.Common;
using System;
using System.Data.Common;

namespace Lotech.Data.SqlServers
{
    using static SqlServerDatabase;
    class DeleteOperationBuilder<TEntity> : CommonDeleteOperationBuilder<TEntity>, IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>
        where TEntity : class
    {
        public DeleteOperationBuilder() : base(Quote, BuildParameter) { }
    }
}
