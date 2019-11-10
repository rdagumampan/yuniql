namespace ArdiLabs.Yuniql
{
    public interface ICsvImportServiceFactory
    {
        ICsvImportService Create(string platform);
    }
}