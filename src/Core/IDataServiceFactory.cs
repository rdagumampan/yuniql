namespace ArdiLabs.Yuniql
{
    public interface IDataServiceFactory
    {
        IDataService Create(string platform);
    }
}