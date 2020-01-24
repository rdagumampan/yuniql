namespace Yuniql.Core
{
    /// <summary>
    /// Interface for implementing helper class for extracting environment variables.
    /// </summary>
    public interface IEnvironmentService
    {
        /// <summary>
        /// Returns the current directory where yuniql is executed from.
        /// </summary>
        string GetCurrentDirectory();

        /// <summary>
        /// Extract values of environment variable with respect to OS platform.
        /// For Windows, the order of ENV variable search is Machine -> User -> Process.
        /// For Linux, it will always use Process.
        /// </summary>
        /// <param name="name">Environment varible name.</param>
        /// <returns>Value of the environment variable.</returns>
        string GetEnvironmentVariable(string name);
    }
}
