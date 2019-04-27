using System;

namespace Lotech.Data.Benchmark
{
    public class BenchmarkDataModel
    {
        [Column(PrimaryKey = true, DbGenerated = true)]
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public DateTime CreateTime { get; set; }

        [Column(DbGenerated = true)]
        public DateTime? ModifyTime { get; set; }

        public bool Deleted { get; set; }

        public byte[] Content { get; set; }

    }
}
