using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.PlatformTests.Interfaces
{
    public interface IMigrationServiceFactory
    {
        IMigrationService Create(string platform);
    }
}