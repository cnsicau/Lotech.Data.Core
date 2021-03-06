﻿using Lotech.Data.Operations;
using Lotech.Data.Operations.Common;
using System;
using System.Data.Common;

namespace Lotech.Data.Oracles
{
    using static OracleDatabase;
    class DeleteOperationBuilder<TEntity> : CommonDeleteOperationBuilder<TEntity>, IOperationBuilder<Action<IDatabase, DbCommand, TEntity>>
        where TEntity : class
    {
        public DeleteOperationBuilder() : base(Quote, BuildParameter) { }
    }
}
