namespace Lotech.Data.Example
{
    public interface IDatabaseExample
    {
        IDatabase Database { get; }

        PageData<Example> PageExecute(ISqlQuery query, Page page);
    }
}
