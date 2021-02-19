using Yuniql.Extensibility;

namespace Yuniql.CLI
{
    public interface IDataServiceFactory
    {
        IDataService Create(string platform);
    }

}