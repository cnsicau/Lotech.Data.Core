using System;

namespace Lotech.Data.Example
{
    public class TestSqlExecutes
    {
        private readonly IDatabase db;

        public TestSqlExecutes(IDatabase db)
        {
            this.db = db;
        }

        public void ExecuteDataSetTest()
        {
            var ds = db.ExecuteDataSet("SELECT * FROM example");
            Console.WriteLine($"db.ExecuteDataSet(\"SELECT * FROM example\") => ds.Tables[0].Rows's count = {ds.Tables?[0].Rows.Count}");
        }

        public void ExecuteNonQueryTest()
        {
            var count = db.ExecuteScalar<int>("SELECT count(*) FROM example");
            Console.WriteLine($"db.ExecuteScalar<int>(\"SELECT count(*) FROM example\") => count = {count}");
        }
    }
}
