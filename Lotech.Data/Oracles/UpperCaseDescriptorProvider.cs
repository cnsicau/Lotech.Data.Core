using Lotech.Data.Descriptors;

namespace Lotech.Data.Oracles
{
    /// <summary>
    /// 全大写名称
    /// </summary>
    class UpperCaseDescriptorProvider : IDescriptorProvider
    {
        class UpperCaseEntityDescriptor<TEntity> : EntityDescriptor where TEntity : class
        {
            internal static readonly UpperCaseEntityDescriptor<TEntity> Instance = new UpperCaseEntityDescriptor<TEntity>();

            UpperCaseEntityDescriptor():base(ReflectionEntityDescriptor<TEntity>.Prototype)
            {
                Name = Name?.ToUpper();
                Schema = Schema?.ToUpper();

                foreach (var member in Members)
                {
                    member.Name = member.Name?.ToUpper();
                }
            }
        }


        IEntityDescriptor IDescriptorProvider.GetEntityDescriptor<TEntity>(Operation operation)
        {
            return UpperCaseEntityDescriptor<TEntity>.Instance;
        }
    }
}
