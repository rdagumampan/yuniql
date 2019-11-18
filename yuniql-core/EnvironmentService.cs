using System;

namespace Yuniql.Core
{
    public class EnvironmentService: IEnvironmentService
    {
        //extracts the environment variable with special consideration when its running on windows
        //https://docs.microsoft.com/en-us/dotnet/api/system.environment.getenvironmentvariable?view=netcore-3.0
        public string GetEnvironmentVariable(string name)
        {
            string result = null;
            if (string.IsNullOrEmpty(result) && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                result = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);

                if (string.IsNullOrEmpty(result))
                    result = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);

                if (string.IsNullOrEmpty(result))
                    result = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
            }
            else
            {
                result = Environment.GetEnvironmentVariable(name);
            }

            return result;
        }
    }
}
