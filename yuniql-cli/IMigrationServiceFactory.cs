using Yuniql.Core;

namespace Yuniql.CLI
{
    public interface IMigrationServiceFactory
    {
        IMigrationService Create(string platform);
    }

}