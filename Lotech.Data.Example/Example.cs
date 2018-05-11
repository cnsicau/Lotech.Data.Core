using System;

namespace Lotech.Data.Test
{

    /*  create database example;

        create table Example
        (
          Id bigint not null primary key identity(1, 1),
          Code nvarchar(32) not null,
          Name nvarchar(128) not null,
          CreateTime datetime not null,
          ModifyTime datetime,
          Deleted bit not null,
          LongId as 100000000 + Id,
          Bin varbinary(max) null
        );
    */
    class Example
    {
        [Column(PrimaryKey = true, DbGenerated = true)]
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? ModifyTime { get; set; }

        [Column(System.Data.DbType.Int16)]
        public bool Deleted { get; set; }

        [Column(DbGenerated = true)]
        public long LongId { get; set; }

        [Column("Bin")]
        public byte[] Content { get; set; }
    }
}
