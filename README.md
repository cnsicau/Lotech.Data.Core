# Lotech.Data.Core
an ORM like "EnterpriseLib Data Block" for dotnet

## 使用
###    引用 `Lotech.Data.dll`
 * 前往 [nuget.org](https://www.nuget.org/packages/Lotech.Data.Core) 手工下载
 * 或 通过 Visual Studio 包管理器控制台安装
``` 
  Install-Package Lotech.Data.Core
```
###    配置 **```database.xml```**
  配置包含驱动、连接串及默认设置内容，提供给 ```DatabaseFactory.Create()``` 、```DatabaseFactory.Create(string connectionName)``` 使用
```xml
<database>
    <dbProviderFactories>
        <!--NET CORE 中注册 SqlServer 驱动, name 被连接串的 providerName 引用-->
        <add name="System.Data.SqlClient" type="System.Data.SqlClient.SqlClientFactory, System.Data.SqlClient"/>        
    </dbProviderFactories>

    <connectionStrings>
        <add name="connection" providerName="System.Data.SqlClient"
             connectionString="Data Source=.; User Id=sa; Password=123456; Initial Catalog = example" />
    </connectionStrings>

    <!-- DatabaseFactory.Create() 时将使用以下配置的默认连接串名称-->
    <databaseSettings defaultDatabase="connection" />
</database>
```

## Api使用
###  实例化 ```IDatabase```

* 通过 __```DatabaseFactory```__ 创建实例

```csharp
  var db = new DatabaseFactory.Create();  // 使用默认连接创建
```
```csharp
  var db = new DatabaseFactory.Create("connection");  // 使用指定的连接名创建
```
* 直接创建对应实例
```csharp
  var db = new SqlServerDatabase(System.Data.SqlClient.SqlClientFactory.Instance) {
    ConnectionString = "Data Source=.; User Id=sa; Password=123456; Initial Catalog = example; 
  };
```

### 配置SQL日志输出 `Log`
```csharp
var db = DatabaseFactory.Create();
db.Log = message => Console.WriteLine(log);     // 将SQL执行日志输出到控制台
```

###  获取 `DbCommand`
  * 获取**给定**类型的 `DbCommand` 对象
```csharp
    public DbCommand GetCommand(CommandType commandType, string commandText);
```
  **示例**:
```csharp
  var sqlCommand = db.GetCommand(CommandType.Text, "SELECT * FROM example");
  var procedureCommand = db.GetCommand(CommandType.Procedure, "MyProc");
```
  * 获取``Text``类型的 `DbCommand` 对象
```csharp
    public DbCommand GetSqlStringCommand(string commandText);
```
  **示例**:
```csharp
  var sqlCommand = db.GetSqlStringCommand( "SELECT * FROM example");
```
### 执行存储过程 `Procedure`
```csharp
using Lotech.Data;

class MyDAO {
  IDatabase db = DatabaseFactory.Create(); // use default database
  
  public MyEntity[] GetMyEntities(string userId, DateTime createDate) {
      // exec MyProc @userId, @date
      // db.ProcedureQuery is extension method, using Lotech.Data is required.
      return db.ProcedureQuery("MyProc", new {userId, date = createDate}).ExecuteEntities<MyEntity>();
  }
}
```