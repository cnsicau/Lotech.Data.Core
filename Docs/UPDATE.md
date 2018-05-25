## 数据更新 `Update`
 
* ####  [单条更新](#1-单条更新-top)
  > ##### [`UpdateEntity`](#11-更新-updateentity)
  > ##### [`UpdateEntityExclude`](#12-排除更新-updateentityexclude)
  > ##### [`UpdateEntityInclude`](#13-局部更新-updateentityinclude)

* #### [批量更新](#2-批量更新-top)
  > ##### [`UpdateEntities`](#21-更新-updateentities)
  > ##### [`UpdateEntitiesExclude`](#22-排除更新-updateentitiesexclude)
  > ##### [`UpdateEntitiesInclude`](#23-局部更新-updateentitiesinclude)

* #### [条件更新](#3-条件更新-top)
  > ##### [`UpdateEntities`](#31-原生写法-updateentities)
  > ##### [`Update`](#32-扩展写法-update)

#

### 示例代码中 `MyData` 的定义
```csharp
public class MyData
{
    public int KeyId { get; set; }

    public string Code { get; set; }

    public string Name { get;set; }

    public bool Deleted { get;set; }

    public DateTime ModifyTime { get; set; }
}
```

#

### **1 单条更新** [[Top](#数据更新-update)]


#### 1.1 更新 `UpdateEntity`
用于更新模型中除主键外的各个字段。
* 声明
```csharp
/// <summary>
/// 更新实体(UPDATE)
/// </summary>
/// <typeparam name="EntityType"></typeparam>
/// <param name="entity"></param>
void UpdateEntity<EntityType>(EntityType entity) where EntityType : class;
```

* **示例**
 ```csharp
   var model = new MyData();
   model.KeyId = 1;
   model.Code = "Test";
   model.Name = "测试";
   model.Deleted = 0;
   model.ModifyTime = DateTime.Now;

   db.UpdateEntity(model);
 ```
其中 `db.UpdateEntity(model)` 基于的 `SQL` 调用代码如下：
```csharp
var sql = "UPDATE MyData SET Code = @code, Name = @name, Deleted = @deleted, ModifyTime = @modifyTime WHERE KeyId = @keyId";
using(var update = db.GetSqlStringCommand(sql)) {
  db.AddInParameter(update, "@code", DbType.String, model.Code);
  db.AddInParameter(update, "@name", DbType.String, model.Name);
  db.AddInParameter(update, "@deleted", DbType.Boolean, model.Deleted);
  db.AddInParameter(update, "@modifyTime", DbType.DateTime, model.ModifyTime);
  db.AddInParameter(update, "@keyId", DbType.Int32, model.KeyId);

  db.ExecuteNonQuery(update);
}
```

#### 1.2 排除更新 `UpdateEntityExclude`
用于排除部分字段不更新，如修改数据时排除 `Code`、`Name` 的内容
* 声明
```csharp
/// <summary>
/// 更新除指定内容部分
/// </summary>
/// <typeparam name="EntityType"></typeparam>
/// <typeparam name="TExclude"></typeparam>
/// <param name="entity"></param>
/// <param name="exclude">排除部分，如  _ => new { _.Code, _.CreateTime } </param>
void UpdateEntityExclude<EntityType, TExclude>(EntityType entity, Func<EntityType, TExclude> exclude) where EntityType : class where TExclude : class;
```

* **示例**
 ```csharp
   var model = new MyData();
   model.KeyId = 1;
   model.Code = "Test";
   model.Name = "测试";
   model.Deleted = 0;
   model.ModifyTime = DateTime.Now;
   // Code 、Name 不被更新
   db.UpdateEntityExclude(model, _ => new { _.Code, _.Name }); 
 ```
其中 `db.UpdateEntityExclude(model, _ => new { _.Code, _.Name })` 基于的 `SQL` 调用代码如下：
```csharp
var sql = "UPDATE MyData SET Deleted = @deleted, ModifyTime = @modifyTime WHERE KeyId = @keyId";
using(var update = db.GetSqlStringCommand(sql)) {
  db.AddInParameter(update, "@deleted", DbType.Boolean, model.Deleted);
  db.AddInParameter(update, "@modifyTime", DbType.DateTime, model.ModifyTime);
  db.AddInParameter(update, "@keyId", DbType.Int32, model.KeyId);

  db.ExecuteNonQuery(update);
}
```

#### 1.3 局部更新 `UpdateEntityInclude`
用于仅更新部分字段，如做逻辑删除时修改数据时的 `Deleted`、`ModifyTime` 内容
* 声明
```csharp
/// <summary>
/// 仅更新包含部分
/// </summary>
/// <typeparam name="EntityType"></typeparam>
/// <typeparam name="TInclude"></typeparam>
/// <param name="entity"></param>
/// <param name="include">仅更新部分，如  _ => new { _.ModifyTime, _.Deleted } </param>
void UpdateEntityInclude<EntityType, TInclude>(EntityType entity, Func<EntityType, TInclude> include) where EntityType : class where TInclude : class;
```

* **示例**
 ```csharp
   var model = new MyData();
   model.KeyId = 1;
   model.Code = "Test";
   model.Name = "测试";
   model.Deleted = 0;
   model.ModifyTime = DateTime.Now;
   // Deleted、ModifyTime被更新
   db.UpdateEntityInclude(model, _ => new { _.Deleted, _.ModifyTime }); 
 ```
其中 `db.UpdateEntityInclude(model, _ => new { _.Deleted, _.ModifyTime })` 基于的 `SQL` 调用代码如下：
```csharp
var sql = "UPDATE MyData SET Deleted = @deleted, ModifyTime = @modifyTime WHERE KeyId = @keyId";
using(var update = db.GetSqlStringCommand(sql)) {
  db.AddInParameter(update, "@deleted", DbType.Boolean, model.Deleted);
  db.AddInParameter(update, "@modifyTime", DbType.DateTime, model.ModifyTime);
  db.AddInParameter(update, "@keyId", DbType.Int32, model.KeyId);

  db.ExecuteNonQuery(update);
}
```
#

### **2 批量更新** [[Top](#数据更新-update)]
#### 2.1 更新 `UpdateEntities`
用于批量更新模型中除主键外的各个字段。
* 声明
```csharp
/// <summary>
/// 批量更新
/// </summary>
/// <typeparam name="EntityType"></typeparam>
/// <param name="entities"></param>
void UpdateEntities<EntityType>(IEnumerable<EntityType> entities) where EntityType : class;
```

* **示例**
 ```csharp
   var model = new MyData();
   model.KeyId = 1;
   model.Code = "Test";
   model.Name = "测试";
   model.Deleted = 0;
   model.ModifyTime = DateTime.Now;

   var models = new List<MyData>(new []{ model });

   db.UpdateEntities(new []{ models });
 ```
其中 `db.UpdateEntities(models)` 基于的 `SQL` 调用代码如下：
```csharp
var sql = "UPDATE MyData SET Code = @code, Name = @name, Deleted = @deleted, ModifyTime = @modifyTime WHERE KeyId = @keyId";
using(var update = db.GetSqlStringCommand(sql)) {
  foreach(var model in models) {
    update.Parameters.Clear();

    db.AddInParameter(update, "@code", DbType.String, model.Code);
    db.AddInParameter(update, "@name", DbType.String, model.Name);
    db.AddInParameter(update, "@deleted", DbType.Boolean, model.Deleted);
    db.AddInParameter(update, "@modifyTime", DbType.DateTime, model.ModifyTime);
    db.AddInParameter(update, "@keyId", DbType.Int32, model.KeyId);

    db.ExecuteNonQuery(update);
  }
}
```

#### 2.2 排除更新 `UpdateEntitiesExclude`
用于批量排除部分字段不更新，如修改数据时排除 `Code`、`Name` 的内容
* 声明
```csharp
/// <summary>
/// 更新除指定内容部分
/// </summary>
/// <typeparam name="EntityType"></typeparam>
/// <typeparam name="TExclude"></typeparam>
/// <param name="entities"></param>
/// <param name="exclude">排除部分，如  _ => new { _.Code, _.CreateTime } </param>
void UpdateEntitiesExclude<EntityType, TExclude>(IEnumerable<EntityType> entities, Func<EntityType, TExclude> exclude) where EntityType : class where TExclude : class;
```

* **示例**
 ```csharp
   var model = new MyData();
   model.KeyId = 1;
   model.Code = "Test";
   model.Name = "测试";
   model.Deleted = 0;
   model.ModifyTime = DateTime.Now;
   var models = new List<MyData>(new []{ model });
   // Code 、Name 不被更新
   db.UpdateEntitiesExclude(models, _ => new { _.Code, _.Name }); 
 ```
其中 `db.UpdateEntitiesExclude(models, _ => new { _.Code, _.Name })` 基于的 `SQL` 调用代码如下：
```csharp
var sql = "UPDATE MyData SET Deleted = @deleted, ModifyTime = @modifyTime WHERE KeyId = @keyId";
using(var update = db.GetSqlStringCommand(sql)) {
  foreach(var model in models) {
    update.Parameters.Clear();

    db.AddInParameter(update, "@deleted", DbType.Boolean, model.Deleted);
    db.AddInParameter(update, "@modifyTime", DbType.DateTime, model.ModifyTime);
    db.AddInParameter(update, "@keyId", DbType.Int32, model.KeyId);

    db.ExecuteNonQuery(update);
  }
}
```

#### 2.3 局部更新 `UpdateEntityInclude`
用于仅更新部分字段，如做逻辑删除时修改数据时的 `Deleted`、`ModifyTime` 内容
* 声明
```csharp
/// <summary>
/// 仅更新包含部分
/// </summary>
/// <typeparam name="EntityType"></typeparam>
/// <typeparam name="TInclude"></typeparam>
/// <param name="entities"></param>
/// <param name="include">仅更新部分，如  _ => new { _.ModifyTime, _.Deleted } </param>
void UpdateEntitiesInclude<EntityType, TInclude>(IEnumerable<EntityType> entities, Func<EntityType, TInclude> include) where EntityType : class where TInclude : class;
```

* **示例**
 ```csharp
   var model = new MyData();
   model.KeyId = 1;
   model.Code = "Test";
   model.Name = "测试";
   model.Deleted = 0;
   model.ModifyTime = DateTime.Now;
   var models = new List<MyData>(new []{ model });
   // Deleted、ModifyTime 被更新
   db.UpdateEntitiesInclude(models, _ => new { _.Deleted, _.ModifyTime }); 
 ```
其中 `db.UpdateEntitiesInclude(models, _ => new { _.Deleted, _.ModifyTime })` 基于的 `SQL` 调用代码如下：
```csharp
var sql = "UPDATE MyData SET Deleted = @deleted, ModifyTime = @modifyTime WHERE KeyId = @keyId";
using(var update = db.GetSqlStringCommand(sql)) {
  foreach(var model in models) {
    update.Parameters.Clear();

    db.AddInParameter(update, "@deleted", DbType.Boolean, model.Deleted);
    db.AddInParameter(update, "@modifyTime", DbType.DateTime, model.ModifyTime);
    db.AddInParameter(update, "@keyId", DbType.Int32, model.KeyId);

    db.ExecuteNonQuery(update);
  }
}
```
#

### **3 条件更新** [[Top](#数据更新-update)]
#### 3.1 原生写法 `UpdateEntities`
用于批量更新满足给定条件数据的部分字段
* 声明
```csharp
/// <summary>
/// 按条件批量更新数据
/// </summary>
/// <typeparam name="EntityType"></typeparam>
/// <typeparam name="TSet"></typeparam>
/// <param name="entity">要更新的数据模板</param>
/// <param name="sets">更新字段过滤，如 _ => new {_.Deleted, _.ModifyTime} </param>
/// <param name="predicate">条件</param>
void UpdateEntities<EntityType, TSet>(EntityType entity, Func<EntityType, TSet> sets, Expression<Func<EntityType, bool>> predicate) where EntityType : class where TSet : class;
```

* **示例**
 ```csharp
   var model = new MyData();
   model.KeyId = 1;
   model.Code = "Test";
   model.Name = "测试";
   model.Deleted = 0;
   model.ModifyTime = DateTime.Now;

   // 将所有 KeyId > 10 的所有数据更新 Deleted、ModifyTime 
   db.UpdateEntities(model, _ => new { _.Deleted, _.ModifyTime }, _ => _.KeyId > 10); 
 ```

其中 `db.UpdateEntities(model, _ => new { _.Deleted, _.ModifyTime }, _ => _.KeyId > 10);` 基于的 `SQL` 调用代码如下：
```csharp
var sql = "UPDATE MyData SET Deleted = @p_0, ModifyTime = @p_1 WHERE KeyId > @p_2";
using(var update = db.GetSqlStringCommand(sql)) {
    update.Parameters.Clear();

    db.AddInParameter(update, "@p_0", DbType.Boolean, model.Deleted);
    db.AddInParameter(update, "@p_1", DbType.DateTime, model.ModifyTime);
    db.AddInParameter(update, "@p_2", DbType.Int32, 10);

    db.ExecuteNonQuery(update);
}
```

#### 3.2 扩展写法 `Update`
用于简化原生方法调用的复杂度，提高可读性.
* **声明**
```csharp
/// <summary>
/// 扩展 Update 实现
///     db.Update&lt;TEntity&gt;().Set(new {Deleted = true, ModifyTime = DateTime.Now}).Where(_ => _.KeyId == 5);
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <param name="database"></param>
/// <returns></returns>
static public UpdateBuilder<TEntity> Update<TEntity>(this IDatabase database) where TEntity : class, new();
```
* **示例**
 ```csharp
   var model = new MyData();
   model.KeyId = 1;
   model.Code = "Test";
   model.Name = "测试";
   model.Deleted = 0;
   model.ModifyTime = DateTime.Now;

   db.Update<MyData>().Set(new {model.Deleted, model.ModifyTime}).Where(_=>_.KeyId > 10);
```
该扩展方法`db.Update<MyData>().Set(new {model.Deleted, model.ModifyTime}).Where(_=>_.KeyId > 10);`调用结果与上述原生`db.UpdateEntities(model, _ => new { _.Deleted, _.ModifyTime }, _ => _.KeyId > 10); `调用效果一致。