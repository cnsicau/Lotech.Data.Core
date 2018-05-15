using System;

namespace Lotech.Data.Example
{
    public class OracleExample : IDatabaseExample
    {
        [Entity("EXAMPLE")]
        internal class Example : IExample
        {
            [Column("ID", PrimaryKey = true, DbGenerated = true)]
            public long Id { get; set; }
            [Column("CODE")]
            public string Code { get; set; }

            [Column("NAME")]
            public string Name { get; set; }

            [Column("CREATETIME")]
            public DateTime CreateTime { get; set; }

            [Column("MODIFYTIME")]
            public DateTime? ModifyTime { get; set; }

            [Column("DELETED", System.Data.DbType.Int16)]
            public bool Deleted { get; set; }

            [Column("LONGID", DbGenerated = true)]
            public long LongId { get; set; }

            [Column("BIN")]
            public byte[] Content { get; set; }
        }

        IDatabase db = DatabaseFactory.CreateDatabase("oracle");

        IDatabase IDatabaseExample.Database => db;

        public OracleExample()
        {
            db.ExecuteNonQuery("truncate table example");
            db.ExecuteNonQuery("drop sequence sexample");
            db.ExecuteNonQuery("create sequence sexample");
        }
    }
}
