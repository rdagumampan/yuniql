using System.Collections.Generic;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    public interface IConfigurationService
    {
        Configuration GetConfiguration();

        void Initialize();

        void Validate();

        string PrintAsJson();

        string GetValueOrDefault(string receivedValue, string environmentVariableName, string defaultValue = null);
    }

}
