namespace Yuniql.Core
{
    public interface IConfigurationService
    {
        Configuration Initialize(Configuration configuration);

        string GetValueOrDefault(string receivedValue, string environmentVariableName, string defaultValue = null);

        Configuration GetConfiguration();

        void Validate();

        string PrintAsJson();
    }

}
