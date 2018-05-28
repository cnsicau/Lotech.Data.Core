## IProcedureQuery

用于直接调用`存储过程`并获取需要的结果.

```csharp
// sp_Example @uid, @name
var sp = db.ProcedureQuery("sp_Example", new {uid = form.userId, form.name});
```

需要获取存储过程返回值时，需要使用原生 DbCommand 方式执行，可通过 IProcedureQuery先绑定输入参数，后通过 AddOutParameter 添加返回.
```csharp
// sp_example_output @uid, @name, @out_count
using(var command = db.ProcedureQuery("sp_Example", new {uid = form.userId, form.name}).CreateCommand()) {
    db.AddOutParameter(command, "@out_count", DbType.Int32, sizeof(int));
    var examples = db.ExecuteEntities<Example>(command);
    var count = Convert.ToInt32(command.Parameters["@out_count"].Value);
}
```

### 执行 IProcedureQuery

用于完成`存储过程`执行，并获取需要的结果。

    var query = db.ProcedureQuery("sp_Example", new {uid = form.userId, form.name});

    DataSet dataSet = query.ExecuteDataSet();

    object scalar = query.ExecuteScalar();

    MyData data = query.ExecuteScalar<MyData>();

    IDataReader reader = query.ExecuteReader();

    dynamic[] dynamics = query.ExecuteEntities();

    MyData[] datas = query.ExecuteEntities<MyData>();

    dynamic dynamicData = query.ExecuteEntity();

    MyData entity = query.ExecuteEntity<MyData>();

    int ret = query.ExecuteNonQuery();

### 其他

基于 `IDatabase` 和 `IProcedureQuery` 执行获取结果的简洁写法，便于简单直接调用并获取结果。

```csharp
    var dataSet = db.ExecuteProcedureDataSet("sp_Example", new {uid = 2, name = "example"});

    object scalar = db.ExecuteProcedureScalar("sp_Example", new {uid = 2, name = "example"});

    IDataReader reader = db.ExecuteProcedureReader("sp_Example", new {uid = 2, name = "example"});

    dynamic[] dynamics = db.ExecuteProcedureEntities("sp_Example", new {uid = 2, name = "example"});

    dynamic dynamicData = db.ExecuteProcedureEntity("sp_Example", new {uid = 2, name = "example"});

    int ret = db.ExecuteProcedureNonQuery("sp_Example", new {uid = 2, name = "example"});
```
鉴于调用中已有匿名参数类型 `TParameter`，因此简洁写法未将 `ExecuteEntity<TEntity>` 和 `ExecuteEntities<TEntity>` 简化，仍按标准写法编写。
```csharp
Example example = db.ProcedureQuery("sp_Example", new {uid = 2, name = "example"}).ExecuteEntity<Example>();

Example[] examples = db.ProcedureQuery("sp_Example", new {uid = 2, name = "example"}).ExecuteEntities<Example>();
```