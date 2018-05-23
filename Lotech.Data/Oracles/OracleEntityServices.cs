using Lotech.Data.Operations;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lotech.Data.Oracles
{
    class OracleEntityServices : IEntityServices
    {
        public IDatabase Database { get; set; }

        public Func<IDatabase, Expression<Func<EntityType, bool>>, int> CountByPredicate<EntityType>() where EntityType : class
        {
            return Operation<EntityType, Func<IDatabase, Expression<Func<EntityType, bool>>, int>, OracleCountEntitiesExpression<EntityType>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.None);
        }

        public Func<IDatabase, int> Count<EntityType>() where EntityType : class
        {
            return Operation<EntityType, Func<IDatabase, int>, OracleCountEntities<EntityType>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.None);
        }
        
        public Action<IDatabase, IEnumerable<TEntity>> DeleteEntities<TEntity>() where TEntity : class
        {
            return Operation<TEntity, Action<IDatabase, IEnumerable<TEntity>>,
                    TransactionalOperationProvider<TEntity>.Instance<DeleteOperationBuilder<TEntity>>
                >.Instance(Database.DescriptorProvider, Descriptors.Operation.Delete);
        }

        public Action<IDatabase, Expression<Func<TEntity, bool>>> DeleteEntitiesByPredicate<TEntity>() where TEntity : class
        {
            return Operation<TEntity, Action<IDatabase, Expression<Func<TEntity, bool>>>, OracleDeleteEntitiesExpression<TEntity>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.Delete);
        }

        public Action<IDatabase, TEntity> DeleteEntity<TEntity>() where TEntity : class
        {
            return Operation<TEntity, Action<IDatabase, TEntity>,
                    OperationProvider<TEntity>.Instance<DeleteOperationBuilder<TEntity>>
                >.Instance(Database.DescriptorProvider, Descriptors.Operation.Delete);
        }

        public Action<IDatabase, TKey> DeleteEntityByKey<TEntity, TKey>() where TEntity : class
        {
            return Operation<TEntity, Action<IDatabase, TKey>, OracleDeleteEntity<TEntity, TKey>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.Delete);
        }

        public Func<IDatabase, TEntity, bool> Exists<TEntity>() where TEntity : class
        {
            return Operation<TEntity, Func<IDatabase, TEntity, bool>, OracleExistsEntity<TEntity>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.None);
        }

        public Func<IDatabase, TKey, bool> ExistsByKey<TEntity, TKey>() where TEntity : class
        {
            return Operation<TEntity, Func<IDatabase, TKey, bool>, OracleExistsEntity<TEntity, TKey>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.None);
        }

        public Func<IDatabase, Expression<Func<TEntity, bool>>, bool> ExistsByPredicate<TEntity>() where TEntity : class
        {
            return Operation<TEntity, Func<IDatabase, Expression<Func<TEntity, bool>>, bool>, OracleExistsEntityExpression<TEntity>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.None);
        }

        public Func<IDatabase, TEntity[]> FindEntities<TEntity>() where TEntity : class
        {
            return Operation<TEntity, Func<IDatabase, TEntity[]>, OracleFindEntities<TEntity>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.Select);
        }

        public Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity[]> FindEntitiesByPredicate<TEntity>() where TEntity : class
        {
            return Operation<TEntity, Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity[]>, OracleFindEntitiesExpression<TEntity>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.Select);
        }
        public Action<IDatabase, IEnumerable<TEntity>> InsertEntities<TEntity>() where TEntity : class
        {
            return Operation<TEntity, Action<IDatabase, IEnumerable<TEntity>>, TransactionalOperationProvider<TEntity>.Instance<InsertOperationBuilder<TEntity>>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.Insert);
        }

        public Action<IDatabase, TEntity> InsertEntity<TEntity>() where TEntity : class
        {
            return Operation<TEntity, Action<IDatabase, TEntity>, OperationProvider<TEntity>.Instance<InsertOperationBuilder<TEntity>>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.Insert);
        }

        public Func<IDatabase, TEntity, TEntity> LoadEntity<TEntity>() where TEntity : class
        {
            return Operation<TEntity, Func<IDatabase, TEntity, TEntity>, OracleLoadEntity<TEntity>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.Select);
        }

        public Func<IDatabase, TKey, TEntity> LoadEntityByKey<TEntity, TKey>() where TEntity : class
        {
            return Operation<TEntity, Func<IDatabase, TKey, TEntity>, OracleLoadEntity<TEntity, TKey>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.Select);
        }

        public Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity> LoadEntityByPredicate<TEntity>() where TEntity : class
        {
            return Operation<TEntity, Func<IDatabase, Expression<Func<TEntity, bool>>, TEntity>, OracleLoadEntityExpression<TEntity>>
                .Instance(Database.DescriptorProvider, Descriptors.Operation.Select);
        }

        public Action<IDatabase, IEnumerable<TEntity>> UpdateEntities<TEntity>() where TEntity : class
        {
            return Operation<TEntity, Action<IDatabase, IEnumerable<TEntity>>,
                    TransactionalOperationProvider<TEntity>.Instance<UpdateOperationBuilder<TEntity>>
                >.Instance(Database.DescriptorProvider, Descriptors.Operation.Update);
        }

        public Action<IDatabase, IEnumerable<TEntity>> UpdateEntitiesExclude<TEntity, TExclude>()
            where TEntity : class
            where TExclude : class
        {
            return Operation<TEntity, Action<IDatabase, IEnumerable<TEntity>>,
                    TransactionalOperationProvider<TEntity>.Instance<UpdateOperationBuilder<TEntity>.Exclude<TExclude>>
                >.Instance(Database.DescriptorProvider, Descriptors.Operation.Update);
        }

        public Action<IDatabase, IEnumerable<TEntity>> UpdateEntitiesInclude<TEntity, TInclude>()
            where TEntity : class
            where TInclude : class
        {
            return Operation<TEntity, Action<IDatabase, IEnumerable<TEntity>>,
                    TransactionalOperationProvider<TEntity>.Instance<UpdateOperationBuilder<TEntity>.Include<TInclude>>
                >.Instance(Database.DescriptorProvider, Descriptors.Operation.Update);
        }

        public Action<IDatabase, TEntity> UpdateEntity<TEntity>() where TEntity : class
        {
            return Operation<TEntity, Action<IDatabase, TEntity>,
                    OperationProvider<TEntity>.Instance<UpdateOperationBuilder<TEntity>>
                >.Instance(Database.DescriptorProvider, Descriptors.Operation.Update);
        }

        public Action<IDatabase, TEntity> UpdateEntityExclude<TEntity, TExclude>()
            where TEntity : class
            where TExclude : class
        {
            return Operation<TEntity, Action<IDatabase, TEntity>,
                    OperationProvider<TEntity>.Instance<UpdateOperationBuilder<TEntity>.Exclude<TExclude>>
                >.Instance(Database.DescriptorProvider, Descriptors.Operation.Update);
        }

        public Action<IDatabase, TEntity> UpdateEntityInclude<TEntity, TInclude>()
            where TEntity : class
            where TInclude : class
        {
            return Operation<TEntity, Action<IDatabase, TEntity>,
                    OperationProvider<TEntity>.Instance<UpdateOperationBuilder<TEntity>.Include<TInclude>>
                >.Instance(Database.DescriptorProvider, Descriptors.Operation.Update);
        }

        public Action<IDatabase, EntityType, Expression<Func<EntityType, bool>>> UpdateEntities<EntityType, TSet>()
            where EntityType : class
            where TSet : class
        {
            return Operation<EntityType, Action<IDatabase, EntityType, Expression<Func<EntityType, bool>>>,
                    OracleUpdateEntities<EntityType, TSet>>.Instance(Database.DescriptorProvider, Descriptors.Operation.Update);
        }
    }
}
