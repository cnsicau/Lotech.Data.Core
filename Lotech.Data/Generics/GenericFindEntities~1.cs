﻿using Lotech.Data.Operations.Common;
using System;

namespace Lotech.Data.Generics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    class GenericFindEntities<TEntity> : CommonFindEntities<TEntity>, IOperationProvider<Func<IDatabase, TEntity[]>>
       where TEntity : class
    {
    }
}
