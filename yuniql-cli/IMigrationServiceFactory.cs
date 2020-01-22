using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.CLI
{
    public interface IMigrationServiceFactory
    {
        IMigrationService Create(string platform);
    }
}