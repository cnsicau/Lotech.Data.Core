namespace Lotech.Data.Test
{
    class Program
    {
        static void Main()
        {
            var sqlite = new SQLiteExample();
            sqlite.TestInsert();
            sqlite.TestLoad();
            sqlite.TestUpdate();
            sqlite.TestExists();
            sqlite.TestDelete();
            sqlite.TestTransaction();
            sqlite.TestFind();

            var mysql = new MySqlExample();
            mysql.TestInsert();
            mysql.TestLoad();
            mysql.TestUpdate();
            mysql.TestExists();
            mysql.TestDelete();
            mysql.TestTransaction();
            mysql.TestFind();

            var oracle = new OracleExample();
            oracle.TestInsert();
            oracle.TestLoad();
            oracle.TestUpdate();
            oracle.TestExists();
            oracle.TestDelete();
            oracle.TestTransaction();
            oracle.TestFind();

            var generic = new GenericExample();
            generic.TestInsert();
            generic.TestLoad();
            generic.TestUpdate();
            generic.TestExists();
            generic.TestDelete();
            generic.TestTransaction();
            generic.TestFind();


            var sql = new SqlServerExample();
            sql.TestInsert();
            sql.TestLoad();
            sql.TestUpdate();
            sql.TestExists();
            sql.TestDelete();
            sql.TestTransaction();
            sql.TestFind();
        }
    }
}
