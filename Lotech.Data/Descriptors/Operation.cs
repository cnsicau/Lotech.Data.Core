namespace Lotech.Data.Descriptors
{
    /// <summary>
    /// 操作枚举
    /// </summary>
    public enum Operation
    {
        /// <summary>
        /// 非标明操作
        /// </summary>
        None,
        /// <summary>
        /// TODO Update有 ExecuteEntity, ExecuteEntities 情况
        /// 查询操作 Load, Find, ExecuteEntities
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
