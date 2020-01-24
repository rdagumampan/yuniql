using System;

namespace Yuniql.Extensibility
{
    /// <summary>
    /// Helper class for extracting environment variables.
    /// </summary>
    public static class EnvironmentHelper
    {
        /// <summary>
        /// Extract values of environment variable with respect to OS platform.
        /// For Windows, the order of ENV variable search is Machine -> User -> Process.
        /// For Linux, it will always use Process.
        /// </summary>
        /// <param name="name">Environment varible name.</param>
        /// <returns>Value of the environment variable.</returns>
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
