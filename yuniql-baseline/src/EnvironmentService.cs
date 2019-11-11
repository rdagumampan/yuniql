using System;

namespace Yuniql.Extensions
{
    public class EnvironmentService : IEnvironmentService
    {
        public string GetEnvironmentVariable(string name)
        {
            string result = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(result) && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                result = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
                if (string.IsNullOrEmpty(result))
                    result = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
            }

            return result;
        }
    }

}
