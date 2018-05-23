using System;
using System.Collections.Concurrent;
using Lotech.Data.Descriptors;

namespace Lotech.Data.Example
{
    using Oracles;

    public class OracleExample : IDatabaseExample
    {
        // 指定 Oracle 全大写描述符提供者
        class OracleDescriptorProvider : IDescriptorProvider
        {
            internal static readonly IDescriptorProvider Instance = new OracleDescriptorProvider();
            static class Descriptors<TEntity>
            {
                internal static readonly ConcurrentDictionary<Operation, IEntityDescriptor> Container = new ConcurrentDictionary<Operation, IEntityDescriptor>();
            }

            public IEntityDescriptor GetEntityDescriptor<TEntity>(Operation operation) where TEntity : class
            {
                return Descriptors<TEntity>.Container.GetOrAdd(operation, _=>
                 {
                     Console.WriteLine("Parse oracle descriptor for " + _ + " " + typeof(TEntity));
                     var descriptor = (EntityDescriptor)DefaultDescriptorProvider.Instance.GetEntityDescriptor<TEntity>(_);
                     descriptor.Name = descriptor.Name.ToUpper();
                     // 转换大写
                     foreach (MemberDescriptor member in descriptor.Members)
                     {
                         if (member.DbType == System.Data.DbType.Boolean)
                             member.DbType = System.Data.DbType.Int16;
                         member.Name = member.Name.ToUpper();
                     }
                     return descriptor;
                 });
            }
        }

        IDatabase db = DatabaseFactory.CreateDatabase("oracle");

        IDatabase IDatabaseExample.Database => db;

        public OracleExample()
        {
            db.DescriptorProvider = OracleDescriptorProvider.Instance;
            db.ExecuteNonQuery("truncate table example");
            db.ExecuteNonQuery("drop sequence sexample");
            db.ExecuteNonQuery("create sequence sexample");
        }

        public PageData<Example> PageExecute(ISqlQuery query, Page page)
        {
            return query.PageExecuteEntites<Example>(page);
        }
    }
}
