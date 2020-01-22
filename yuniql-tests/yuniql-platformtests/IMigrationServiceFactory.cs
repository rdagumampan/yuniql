using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.PlatformTests
{
    public interface IMigrationServiceFactory
    {
        IMigrationService Create(string platform);
    }
}