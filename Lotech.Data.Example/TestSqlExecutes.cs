using System;
using System.Data;
using System.Dynamic;
using System.Linq;

namespace Lotech.Data.Example
{
    public class TestSqlExecutes<TExample> where TExample : class, IExample, new()
    {
        private readonly IDatabase db;
        static void WriteErrorLine(string message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = color;
        }
        public TestSqlExecutes(IDatabase db)
        {
            this.db = db;
            ((DbProviderDatabase)db).EnableTraceLog();
        }
        public void ExecuteDataSetTest()
        {
            var ds = db.ExecuteDataSet("SELECT * FROM example");
            Console.WriteLine($"db.ExecuteDataSet(\"SELECT * FROM example\") => ds.Tables[0].Rows's count = {ds.Tables?[0].Rows.Count}");

            ds = db.ExecuteDataSet(CommandType.Text, "SELECT * FROM example");
            Console.WriteLine($"db.ExecuteDataSet(CommandType.Text, \"SELECT * FROM example\") => ds.Tables[0].Rows's count = {ds.Tables?[0].Rows.Count}");

            try
            {
                ds = db.ExecuteDataSet(CommandType.TableDirect, "example");
                Console.WriteLine($"db.ExecuteDataSet(CommandType.TableDirect, \"example\") => ds.Tables[0].Rows's count = {ds.Tables?[0].Rows.Count}");
            }
            catch (Exception e)
            {
                WriteErrorLine($"db.ExecuteDataSet(CommandType.TableDirect, \"example\") error => {e.Message}");
            }
            using (var command = db.GetSqlStringCommand("SELECT * FROM example"))
            {
                ds = db.ExecuteDataSet(command);
                Console.WriteLine($"db.ExecuteDataSet(command) => ds.Tables[0].Rows's count = {ds.Tables?[0].Rows.Count}");
            }
        }
        public void ExecuteEntitiesTest()
        {
            var entities = db.ExecuteEntities<TExample>("SELECT * FROM example");
            Console.WriteLine($"db.ExecuteEntities<TExample>(\"SELECT * FROM example\") => entities's count = {entities.Length}");

            entities = db.ExecuteEntities<TExample>(CommandType.Text, "SELECT * FROM example");
            Console.WriteLine($"db.ExecuteEntities<TExample>(CommandType.Text, \"SELECT * FROM example\") => entities's count = {entities.Length}");

            try
            {
                entities = db.ExecuteEntities<TExample>(CommandType.TableDirect, "example");
                Console.WriteLine($"db.ExecuteEntities<TExample>(CommandType.TableDirect, \"example\") => entities's count = {entities.Length}");
            }
            catch (Exception e)
            {
                WriteErrorLine($"db.ExecuteEntities<TExample>(CommandType.TableDirect, \"example\") error => {e.Message}");
            }
            using (var command = db.GetSqlStringCommand("SELECT * FROM example"))
            {
                entities = db.ExecuteEntities<TExample>(command);
                Console.WriteLine($"db.ExecuteEntities<TExample>(command) => entities's count = {entities.Length}");
            }
        }
        public void ExecuteEntityTest()
        {
            var example = db.ExecuteEntity<TExample>("SELECT * FROM example");
            Console.WriteLine($"db.ExecuteEntity<TExample>(\"SELECT * FROM example\") => example's name = {example.Name}");

            example = db.ExecuteEntity<TExample>(CommandType.Text, "SELECT * FROM example");
            Console.WriteLine($"db.ExecuteEntity<TExample>(CommandType.Text, \"SELECT * FROM example\") => example's name = {example.Name}");

            try
            {
                example = db.ExecuteEntity<TExample>(CommandType.TableDirect, "example");
                Console.WriteLine($"db.ExecuteEntity<TExample>(CommandType.TableDirect, \"example\") => example's name = {example.Name}");
            }
            catch (Exception e)
            {
                WriteErrorLine($"db.ExecuteEntity<TExample>(CommandType.TableDirect, \"example\") error => {e.Message}");
            }
            using (var command = db.GetSqlStringCommand("SELECT * FROM example"))
            {
                example = db.ExecuteEntity<TExample>(command);
                Console.WriteLine($"db.ExecuteEntity<TExample>(command) => example's name = {example.Name}");
            }
        }
        public void ExecuteDynamicTest()
        {
            var example = db.ExecuteEntity("SELECT * FROM example") ;
            Console.WriteLine($"db.ExecuteEntity(\"SELECT * FROM example\") => example's name = {example.Name}");

            example = db.ExecuteEntity(CommandType.Text, "SELECT * FROM example");
            Console.WriteLine($"db.ExecuteEntity(CommandType.Text, \"SELECT * FROM example\") => example's name = {example.Name}");

            try
            {
                example = db.ExecuteEntity(CommandType.TableDirect, "example");
                Console.WriteLine($"db.ExecuteEntity(CommandType.TableDirect, \"example\") => example's name = {example.Name}");
            }
            catch (Exception e)
            {
                WriteErrorLine($"db.ExecuteEntity(CommandType.TableDirect, \"example\") error => {e.Message}");
            }
            using (var command = db.GetSqlStringCommand("SELECT * FROM example"))
            {
                example = db.ExecuteEntity(command);
                Console.WriteLine($"db.ExecuteEntity(command) => example's name = {example.Name}");
            }
        }

        public void ExecuteScalarTest()
        {
            var scalar = db.ExecuteScalar("SELECT * FROM example");
            Console.WriteLine($"db.ExecuteScalar(\"SELECT * FROM example\") => scalar = {scalar}");

            scalar = db.ExecuteScalar(CommandType.Text, "SELECT * FROM example");
            Console.WriteLine($"db.ExecuteScalar(CommandType.Text, \"SELECT * FROM example\") => scalar = {scalar}");

            try
            {
                scalar = db.ExecuteScalar(CommandType.TableDirect, "example");
                Console.WriteLine($"db.ExecuteScalar(CommandType.TableDirect, \"example\") => scalar = {scalar}");
            }
            catch (Exception e)
            {
                WriteErrorLine($"db.ExecuteScalar(CommandType.TableDirect, \"example\") error => {e.Message}");
            }
            using (var command = db.GetSqlStringCommand("SELECT * FROM example"))
            {
                scalar = db.ExecuteScalar(command);
                Console.WriteLine($"db.ExecuteScalar(command) => scalar = {scalar}");
            }
        }
        public void ExecuteScalarTTest()
        {
            var scalar = db.ExecuteScalar<long>("SELECT * FROM example");
            Console.WriteLine($"db.ExecuteScalar<long>(\"SELECT * FROM example\") => scalar = {scalar}");

            scalar = db.ExecuteScalar<long>(CommandType.Text, "SELECT * FROM example");
            Console.WriteLine($"db.ExecuteScalar<long>(CommandType.Text, \"SELECT * FROM example\") => scalar = {scalar}");

            try
            {
                scalar = db.ExecuteScalar<long>(CommandType.TableDirect, "example");
                Console.WriteLine($"db.ExecuteScalar<long>(CommandType.TableDirect, \"example\") => scalar = {scalar}");
            }
            catch (Exception e)
            {
                WriteErrorLine($"db.ExecuteScalar<long>(CommandType.TableDirect, \"example\") error => {e.Message}");
            }
            using (var command = db.GetSqlStringCommand("SELECT * FROM example"))
            {
                scalar = db.ExecuteScalar<long>(command);
                Console.WriteLine($"db.ExecuteScalar<long>(command) => scalar = {scalar}");
            }
        }
    }
}
