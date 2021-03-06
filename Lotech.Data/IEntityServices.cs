﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lotech.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEntityServices
    {
        /// <summary>
        /// 设置或获取当前服务服务关联的库对象
        /// </summary>
        IDatabase Database { get; set; }

        #region Insert

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        Action<IDatabase, TEntity> InsertEntity<TEntity>() where TEntity : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        Action<IDatabase, IEnumerable<TEntity>> InsertEntities<TEntity>() where TEntity : class;
        #endregion

        #region Update

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        Action<IDatabase, TEntity> UpdateEntity<TEntity>() where TEntity : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        Action<IDatabase, IEnumerable<TEntity>> UpdateEntities<TEntity>() where TEntity : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TInclude"></typeparam>
        /// <returns></returns>
        Action<IDatabase, TEntity> UpdateEntityInclude<TEntity, TInclude>() where TEntity : class where TInclude : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TInclude"></typeparam>
        /// <returns></returns>
        Action<IDatabase, IEnumerable<TEntity>> UpdateEntitiesInclude<TEntity, TInclude>() where TEntity : class where TInclude : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TExclude"></typeparam>
        /// <returns></returns>
        Action<IDatabase, TEntity> UpdateEntityExclude<TEntity, TExclude>() where TEntity : class where TExclude : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TExclude"></typeparam>
        /// <returns></returns>
        Action<IDatabase, IEnumerable<TEntity>> UpdateEntitiesExclude<TEntity, TExclude>() where TEntity : class where TExclude : class;

        #endregion

        #region Delete
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        Action<IDatabase, TKey> DeleteEntityByKey<TEntity, TKey>() where TEntity : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        Action<IDatabase, TEntity> DeleteEntity<TEntity>() where TEntity : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        Action<IDatabase, IEnumerable<TEntity>> DeleteEntities<TEntity>() where TEntity : class;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Action<IDatabase, Expression<Func<TEntity, bool>>> DeleteEntitiesByPredicate<TEntity>() where TEntity : class;
        #endregion

        #region Find

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        Func<IDatabase, TEntity[]> FindEntities<TEntity>() where TEntity : class;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity[]> FindEntitiesByPredicate<TEntity>() where TEntity : class;
        #endregion

        #region Exists
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        Func<IDatabase, TKey, bool> ExistsByKey<TEntity, TKey>() where TEntity : class;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        Func<IDatabase, TEntity, bool> Exists<TEntity>() where TEntity : class;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        Func<IDatabase, Expression<Func<TEntity, bool>>, bool> ExistsByPredicate<TEntity>() where TEntity : class;
        #endregion

        #region LoadEntity
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        Func<IDatabase, TKey, TEntity> LoadEntityByKey<TEntity, TKey>() where TEntity : class;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        Func<IDatabase, TEntity, TEntity> LoadEntity<TEntity>() where TEntity : class;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity> LoadEntityByPredicate<TEntity>() where TEntity : class;

        #endregion

        #region v2.1 UpdateEntitiesX + Count(..)
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="TSet"></typeparam>
        Action<IDatabase, EntityType, Expression<Func<EntityType, bool>>> UpdateEntities<EntityType, TSet>() where EntityType : class where TSet : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <returns></returns>
        Func<IDatabase, int> Count<EntityType>() where EntityType : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <returns></returns>
        Func<IDatabase, Expression<Func<EntityType, bool>>, int> CountByPredicate<EntityType>() where EntityType : class;
        #endregion
    }
}
