using System;

namespace Lotech.Data.Example
{
    public interface IExample
    {
        string Code { get; set; }
        byte[] Content { get; set; }
        DateTime CreateTime { get; set; }
        bool Deleted { get; set; }
        long Id { get; set; }
        long LongId { get; set; }
        DateTime? ModifyTime { get; set; }
        string Name { get; set; }
    }

    /// <summary>
    /// 默认
    /// </summary>
    public class Example : IExample
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