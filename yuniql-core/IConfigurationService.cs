namespace Yuniql.Core
{
    public interface IConfigurationService
    {
        void Initialize(Configuration configuration);

        Configuration GetConfiguration();

        void Validate();

        string PrintAsJson();
    }

}
