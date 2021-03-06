﻿using Lotech.Data.Operations.Common;

namespace Lotech.Data.SQLites
{
    class SQLiteUpdateEntities<TEntity, TSet> : CommonUpdateEntities<TEntity, TSet> where TEntity : class where TSet : class
    {
        public SQLiteUpdateEntities() : base(SQLiteEntityServices.Quote, SQLiteEntityServices.BuildParameter, _ => new SQLiteExpressionVisitor<TEntity>(_, Descriptors.Operation.Update)) { }
    }
}
