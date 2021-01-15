using Yuniql.Extensibility;

namespace Yuniql.PlatformTests.Interfaces
{
    public interface ITestDataServiceFactory
    {
        ITestDataService Create(string platform);
    }
}