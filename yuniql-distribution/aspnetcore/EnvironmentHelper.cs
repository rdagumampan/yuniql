using System;

namespace Yuniql.AspNetCore
{
    /// <summary>
    /// Helper class for extracting environment variables.
    /// </summary>
    public class EnvironmentHelper
    {
        ///<inheritdoc/>
        public static string GetCurrentDirectory()
        {
            return Environment.CurrentDirectory;
        }

        //extracts the environment variable with special consideration when its running on windows
        //https://docs.microsoft.com/en-us/dotnet/api/system.environment.getenvironmentvariable?view=netcore-3.0

        ///<inheritdoc/>
        public static string GetEnvironmentVariable(string name)
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
