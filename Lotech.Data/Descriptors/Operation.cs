namespace Lotech.Data.Descriptors
{
    /// <summary>
    /// 操作枚举
    /// </summary>
    public enum Operation
    {
        /// <summary>
        /// 非标明操作，包括 Count\Exists\ExecuteEntities\....
        /// </summary>
        None,
        /// <summary>
        /// 查询操作 Load, Find
        /// </summary>
        Select,
        /// <summary>
        /// 插入操作  InsertEntity \ InsertEntities
        /// </summary>
        Insert,
        /// <summary>
        /// 更新操作 UpdateEntity \ UpdateEntities
        /// </summary>
        Update,
        /// <summary>
        /// 删除操作 DeleteEntity \ DeleteEntities
        /// </summary>
        Delete,
    }
}
