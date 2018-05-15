using System;

namespace Lotech.Data.Example
{
    public class MySqlExample : IDatabaseExample
    {
        public IDatabase db = DatabaseFactory.CreateDatabase("mysql");

        IDatabase IDatabaseExample.Database => db;

        public MySqlExample()
        {
            db.ExecuteNonQuery("drop table if exists example");
            db.ExecuteNonQuery(@"create table Example (
    Id bigint not null auto_increment primary key,
    Code nvarchar(32) not null,
    Name nvarchar(128) not null,
    CreateTime datetime not null,
    ModifyTime datetime,
    Deleted bit not null,
    LongId bigint as ( createtime ) stored,
    Bin blob null
)");
        }
    }
}
