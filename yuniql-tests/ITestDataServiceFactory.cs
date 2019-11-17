using Yuniql.Extensibility;

namespace Yuniql.Tests
{
    public interface ITestDataServiceFactory
    {
        ITestDataService Create(string platform);
    }
}