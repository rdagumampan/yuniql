namespace Yuniql.Core
{
    /// <summary>
    /// Interface for implementing service responsible for initializing and managing the local workspace. A local workspace is a directory where yuniql operations are operated from.
    /// </summary>
    public interface IWorkspaceService
    {
        /// <summary>
        /// Creates the baseline directory structure in the target workspace path.
        /// </summary>
        /// <param name="workingPath">The directory path where workspace structure will be created.</param>
        void Init(string workingPath);

        /// <summary>
        /// Gets the latest version available in the local workspace.
        /// </summary>
        /// <param name="workingPath">The directory path when yuniql operations are executed from.</param>
        string GetLatestVersion(string workingPath);

        /// <summary>
        /// Creates a new major version migration directory by incrementing the latest major version.
        /// </summary>
        /// <param name="workingPath">The directory path where yuniql operations are executed from.</param>
        /// <param name="sqlFileName">File name of sql file to be created.</param>
        /// <returns>The version created in v{Major}.{Minor} format.</returns>
        string IncrementMajorVersion(string workingPath, string sqlFileName);

        /// <summary>
        /// Creates a new minor version migration directory by incrementing tha latest version.
        /// </summary>
        /// <param name="workingPath">The directory path where yuniql operations are executed from.</param>
        /// <param name="sqlFileName">File name of sql file to be created.</param>
        /// <returns>The version created in v{Major}.{Minor} format.</returns>
        string IncrementMinorVersion(string workingPath, string sqlFileName);

        /// <summary>
        /// Validates the baseline directory structure. The following directories are always required to be present in the workspace else the migration would fail.
        /// Required folders are _init, _pre, v0.00, _draft, _post, and _erase.
        /// </summary>
        /// <param name="workingPath">The directory path where yuniql operations are executed from.</param>
        void Validate(string workingPath);
    }
}