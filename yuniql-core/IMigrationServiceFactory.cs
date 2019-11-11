namespace Yuniql.Core
{
    public interface IMigrationServiceFactory
    {
        IMigrationService Create(string platform);
    }
}