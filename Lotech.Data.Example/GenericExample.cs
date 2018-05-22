using System;

namespace Lotech.Data.Example
{
    class GenericExample : IDatabaseExample
    {
        IDatabase db = DatabaseFactory.CreateDatabase();
        public GenericExample()
        {
            Console.WriteLine(GetType().Name.PadLeft(40, '-').PadRight(80, '-'));

            db.ExecuteNonQuery("truncate table example");
        }

        public IDatabase Database => db;

        public PageData<Example> PageExecute(ISqlQuery query, Page page)
        {
            return null;
        }
    }
}
