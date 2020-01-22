using Yuniql.Extensibility;

namespace Yuniql.PlatformTests
{
    public interface ITestDataServiceFactory
    {
        ITestDataService Create(string platform);
    }
}