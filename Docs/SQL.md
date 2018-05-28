## 拼接SQL

用于查询时**动态拼接SQL**语句，如：
```csharp
var query = db.SqlQuery("SELECT * FROM example")
                .AppendLine(" WHERE {1} < {0}", 3, 2)
                .AppendNotNull(name, " AND Name LIKE {0}")
                .AppendNotNullOrEmpty(code, " AND Code LIKE {0}");
```
> **1.** 当 `name = null` 、 `code = ""`时，可拼接为如下SQL
>```sql
>SELECT * FROM example WHERE @p_0_0 < @p_0_1
> -- @p_0_0 = 2
> -- @p_0_1 = 3
>```
> **2.** 当 `name = null` 、 `code = "A"`时，可拼接为如下SQL
> ```sql
> SELECT * FROM example WHERE @p_0_0 < @p_0_1
>  AND Code LIKE @p_0_2
> -- @p_0_0 = 2
> -- @p_0_1 = 3
> -- @p_0_2 = A
> ```
> **3.**  当 `name = "名称"` 、 `code = "A"`时，可拼接为如下SQL
> ```sql
> SELECT * FROM example WHERE @p_0_0 < @p_0_1
>  AND Name LIKE @p_0_2 AND Code LIKE @p_0_3
> -- @p_0_0 = 2
> -- @p_0_1 = 3
> -- @p_0_2 = 名称
> -- @p_0_3 = A
> ```

  使用 `AppendIn` 扩展方法添加 `In` 多项值
```csharp
    var query = db.SqlQuery("SELECT * FROM example WHERE 1 = 1")
            .AppendIn(" AND KeyId IN ({0})", new[] { 1, 2, 3, 4, 5 })
            .AppendIn(" AND KeyId IN ({0})", new[] { 1 })
            .AppendIn(" AND KeyId IN ({0})", new int[0])
            .AppendIn(" AND KeyId IN ({0})", (int[])null)
            .AppendIn(" AND Code IN ({0})", "0")
            .AppendIn(" AND Code IN ({0})", 'A', 'B', 'C', 'D');
```
拼接结果：
```sql
  SELECT * FROM example WHERE 1 = 1 AND KeyId IN (@p_el_0, @p_el_1, @p_el_2, @p_el_3, @p_el_4) AND KeyId (@p_el_5) AND Code IN (@p_el_6) AND Code IN (@p_el_7, @p_el_8, @p_el_9, @p_el_10)
  --  @p_el_0 = 1
  --  @p_el_1 = 2
  --  @p_el_2 = 3
  --  @p_el_3 = 4
  --  @p_el_4 = 5
  --  @p_el_5 = 1
  --  @p_el_6 = 0
  --  @p_el_7 = A
  --  @p_el_8 = B
  --  @p_el_9 = C
  --  @p_el_10 = D
```

### **创建 `ISqlQuery` 实例**

`IDatabase` 创建 `ISqlQuery` 实例的方法清单如下：

```csharp
    /// <summary>构建无初始SQL的实例</summary>
    ISqlQuery SqlQuery();

    /// <summary>构建指定初始SQL的实例</summary>
    /// <param name="sql">初始SQL语句</param>
    ISqlQuery SqlQuery(string sql);

    /// <summary>构建指定初始SQL、参数实例</summary>
    /// <param name="sql">初始SQL语句，可使用 {0}、{1}…{n}，向后引用args位置上的参数值</param>
    /// <param name="args">用于sql中的参数引用</param>
    ISqlQuery SqlQuery(string sql, params object[] args);

    /// <summary>构建指定初始SQL的实例并在末尾追加换行</summary>
    /// <param name="sql">初始SQL语句</param>
    ISqlQuery SqlQueryLine(string sql);

    /// <summary>构建指定初始SQL、参数实例并在末尾追加换行</summary>
    /// <param name="sql">初始SQL语句，可使用 {0}、{1}…{n}，向后引用args位置上的参数值</param>
    /// <param name="args">用于sql中的参数引用</param>
    ISqlQuery SqlQueryLine(string sql, params object[] args);
```
**注**：方法清单为 `IDatabase` 的扩展，因此需要在使用文件中引通过 `using Lotech.Data;`引入命名空间。

#### 示例
```csharp
    IDatabase db = DatabaseFactory.Create();
    
    var query1 = db.SqlQuery();

    var query2 = db.SqlQuery("SELECT * FROM example");

    var query3 = db.SqlQueryLine("SELECT * FROM example");

    var query4 = db.SqlQuery("SELECT * FROM example WHERE KeyId = {0})", 1);

    var query5 = db.SqlQueryLine("SELECT * FROM example WHERE KeyId = {0})", 1);
```

### **拼接 `ISqlQuery`**
  对已有 SqlQuery 追加更多SQL片断及参数.
```csharp
var query = db.SqlQuery("SELECT * FROM example WHERE 1 = 1");

query.Append(" And Code LIKE 'T%'");
query.Append(" AND Code LIKE {0}", "T%");
query.Append(" AND Code BETWEEN {0} AND {1}", "T0", "TZ");

query.AppendLine();

query.AppendLine(" AND KeyId > 0")
query.AppendLine(" AND KeyId > {0}", 1);
query.AppendLine(" AND KeyId BETWEEN {0} AND {1}", 2, 10);


query.AppendRaw(" AND Name LIKE @name", new SqlQueryParameter("@name", "测试%"));
query.AppendLineRaw(" AND Name IN (@name1, @name2)", 
   new SqlQueryParameter("@name1", "测试A"),
   new SqlQueryParameter("@name2", "测试Z"));
```
###

   条件追加SQL片断及参数. `AppendNotNull` `AppendNotNullOrEmpty`。

```csharp
// 当 name 不为 null 时，执行 Append(" AND Name LIKE {0} + '%'", name) 逻辑
s.AppendNotNull(name, " AND Name LIKE {0} + '%'");

// 当 name 不为 null 和 空字符串 时，执行 Append(" AND Name LIKE {0} + '%'", name) 逻辑
s.AppendNotNull(name, " AND Name LIKE {0} + '%'");
```


### **执行 `ISqlQuery`**
  用于完成SQL执行，并获取需要的结果。

```csharp
    var query = db.SqlQuery("SELECT * FROM example")
                    .Append(" WHERE KeyId BETWEEN {0} AND {1}", 2, 10);

    DataSet dataSet = query.ExecuteDataSet();

    object scalar = query.ExecuteScalar();

    MyData data = query.ExecuteScalar<MyData>();

    IDataReader reader = query.ExecuteReader();

    dynamic[] dynamics = query.ExecuteEntities();

    MyData[] datas = query.ExecuteEntities<MyData>();

    dynamic dynamicData = query.ExecuteEntity();

    MyData entity = query.ExecuteEntity<MyData>();

    int ret = query.ExecuteNonQuery();
```

### 其他
  基于 `IDatabase` 和 `ISqlQuery` 执行获取结果的简洁写法，便于简单直接调用SQL并获取结果。

```csharp
    var dataSet = db.ExecuteSqlDataSet("SELECT * FROM example WHERE KeyId BETWEEN {0} AND {1}", 2, 10);

    object scalar = db.ExecuteSqlScalar("SELECT * FROM example WHERE KeyId BETWEEN {0} AND {1}", 2, 10);

    MyData data = db.ExecuteSqlScalar<MyData>("SELECT * FROM example WHERE KeyId BETWEEN {0} AND {1}", 2, 10);

    IDataReader reader = db.ExecuteSqlReader("SELECT * FROM example WHERE KeyId BETWEEN {0} AND {1}", 2, 10);

    dynamic[] dynamics = db.ExecuteSqlEntities("SELECT * FROM example WHERE KeyId BETWEEN {0} AND {1}", 2, 10);

    MyData[] datas = db.ExecuteSqlEntities<MyData>("SELECT * FROM example WHERE KeyId BETWEEN {0} AND {1}", 2, 10);

    dynamic dynamicData = db.ExecuteSqlEntity("SELECT * FROM example WHERE KeyId BETWEEN {0} AND {1}", 2, 10);

    MyData entity = db.ExecuteSqlEntity<MyData>("SELECT * FROM example WHERE KeyId BETWEEN {0} AND {1}", 2, 10);

    int ret = db.ExecuteSqlNonQuery("SELECT * FROM example WHERE KeyId BETWEEN {0} AND {1}", 2, 10);
```