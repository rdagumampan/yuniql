using Yuniql.Extensibility;

namespace Yuniql.CLI
{
    public interface IManifestFactory
    {
        ManifestData Create(string platform);
    }

}