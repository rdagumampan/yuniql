namespace ArdiLabs.Yuniql
{
    public interface IMigrationServiceFactory
    {
        IMigrationService Create(string platform);
    }
}