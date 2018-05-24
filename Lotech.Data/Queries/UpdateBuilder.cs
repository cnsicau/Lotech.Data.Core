using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class UpdateBuilder<TEntity> where TEntity : class, new()
    {
        private readonly IDatabase database;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        internal UpdateBuilder(IDatabase database) { this.database = database; }

        /// <summary>
        /// 设置要更新的值，如 new { CreateTime = DateTime.Now,  Code = code }
        /// </summary>
        /// <typeparam name="TSet"></typeparam>
        /// <param name="set"></param>
        /// <returns></returns>
        public Executer<TSet> Set<TSet>(TSet set) where TSet : class
        {
            return new Executer<TSet>(this, set);
        }

        public class Executer<TSet> where TSet : class
        {
            static readonly Action<TSet, TEntity> assign;

            static bool FilterMember(MemberInfo member)
            {
                return (member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field)
                    && typeof(TSet).GetMember(member.Name).SingleOrDefault() != null;
            }
            static Executer()
            {
                var set = Expression.Parameter(typeof(TSet));
                var entity = Expression.Parameter(typeof(TEntity));
                var bindings = BindingFlags.Public | BindingFlags.Instance;

                var blocks = new List<Expression>();

                var members = typeof(TEntity).GetMembers(bindings).Where(FilterMember);
                foreach (var member in members)
                {
                    blocks.Add(Expression.Assign(
                            Expression.MakeMemberAccess(entity, member),
                            Expression.MakeMemberAccess(set, typeof(TSet).GetMember(member.Name).Single())
                        ));
                }

                assign = Expression.Lambda<Action<TSet, TEntity>>(Expression.Block(blocks), set, entity).Compile();
            }

            private readonly TSet set;
            private readonly UpdateBuilder<TEntity> update;

            internal Executer(UpdateBuilder<TEntity> update, TSet set)
            {
                this.set = set;
                this.update = update;
            }

            public void Where(Expression<Func<TEntity, bool>> predicate)
            {
                var entity = new TEntity();
                assign(set, entity);
                update.database.UpdateEntities(entity, Set, predicate);
            }

            TSet Set(TEntity entity) { return set; }
        }
    }
}
