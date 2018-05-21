using System;
using Lotech.Data.SQLites;

namespace Lotech.Data.Example
{
    public class SQLiteExample : IDatabaseExample
    {
        public class Example : IExample
        {
            [Column(PrimaryKey = true, DbGenerated = true)]
            public long Id { get; set; }

            public string Code { get; set; }

            public string Name { get; set; }

            public DateTime CreateTime { get; set; }

            [Column(DbGenerated = true)]
            public DateTime? ModifyTime { get; set; }
            
            public bool Deleted { get; set; }

            [Column(DbGenerated = true)]
            public long LongId { get; set; }

            [Column("Bin")]
            public byte[] Content { get; set; }
        }

        IDatabase db = DatabaseFactory.CreateDatabase("sqlite");

        public IDatabase Database => db;

        public SQLiteExample()
        {
#if NET_CORE
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
#endif
            db.ExecuteNonQuery("drop table if exists  Example");
            db.ExecuteNonQuery(@"create table Example (
   Id INTEGER PRIMARY KEY AUTOINCREMENT,
   Code nvarchar(32) not null,
   Name nvarchar(128) not null,
   CreateTime datetime not null,
   ModifyTime TIMESTAMP default CURRENT_TIMESTAMP ,
   Deleted bit not null,
   LongId bigint default 10,
   Bin blob null
)");
        }

        PageData<Data.Example.Example> IDatabaseExample.PageExecute(ISqlQuery query, Page page)
        {
            return query.PageExecuteEntites<Data.Example.Example>(page);
        }
    }
}
