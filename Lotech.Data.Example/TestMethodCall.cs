using System;
using System.Linq;

namespace Lotech.Data.Example
{
    public class TestMethodCall<TExample> where TExample : class, IExample, new()
    {
        private readonly IDatabase db;

        public TestMethodCall(IDatabaseExample example)
        {
            db = example.Database;
            ((DbProviderDatabase)db).EnableTraceLog();
        }

        public void Test()
        {
            Console.WriteLine("db.LoadEntity<TExample>(_ => _.Code.ToUpper() == \"T001\")?.Name = "
                + db.LoadEntity<TExample>(_ => _.Code.ToUpper() == "T001")?.Name);
            Console.WriteLine("db.LoadEntity<TExample>(_ => _.Code.ToUpper() == \"t001\")?.Name = "
                + db.LoadEntity<TExample>(_ => _.Code.ToLower() == "t001")?.Name);
            Console.WriteLine("db.LoadEntity<TExample>(_ => _.Code.StartsWith(\"T0\"))?.Name = "
                + db.LoadEntity<TExample>(_ => _.Code.StartsWith("T0"))?.Name);
            Console.WriteLine("db.LoadEntity<TExample>(_ => _.Code.EndsWith(\"01\"))?.Name = "
                + db.LoadEntity<TExample>(_ => _.Code.EndsWith("01"))?.Name);
            Console.WriteLine("db.LoadEntity<TExample>(_ => _.Code.Contains(\"00\"))?.Name = "
                + db.LoadEntity<TExample>(_ => _.Code.Contains("00"))?.Name);
            Console.WriteLine("db.LoadEntity<TExample>(_ => _.Id.ToString() != null)?.Name = "
                + db.LoadEntity<TExample>(_ => _.Id.ToString() != null)?.Name);
            Console.WriteLine("db.LoadEntity<TExample>(_ => _.CreateTime.ToString() != null)?.Name = "
                + db.LoadEntity<TExample>(_ => _.CreateTime.ToString() != null)?.Name);

            Console.WriteLine("db.LoadEntity<TExample>(_ => _.CreateTime.ToString().Substring(1, 4).Contains(\"0\"))?.Name = "
                + db.LoadEntity<TExample>(_ => _.CreateTime.ToString().Substring(1, 4).Contains("0"))?.Name);


            Console.WriteLine("db.LoadEntity<TExample>(_ => _.Code.ToUpper().Contains(\"00\"))?.Name = "
                + db.LoadEntity<TExample>(_ => _.Code.ToUpper().Contains("00"))?.Name);
            var codes = new string[] { "t001", "T001", "T002" };

            db.LoadEntity<TExample>(_ => codes.Contains(_.Code.ToLower()));
        }
    }
}
