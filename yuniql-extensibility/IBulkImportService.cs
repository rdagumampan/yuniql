using System.Data;

namespace Yuniql.Extensibility
{
    /// <summary>
    /// Implement this interface to support bulk import on a target database platform or provider.
    /// </summary>
    public interface IBulkImportService
    {
        /// <summary>
        /// Initialize the bulk import service. Sets connection string for future operations.
        /// </summary>
        /// <param name="connectionString">Connection string to the target database.</param>
        void Initialize(
            string connectionString);

        /// <summary>
        /// Runs the bulk import process using custom or native APIs in the target database platform.
        /// </summary>
        /// <param name="connection">Connection to target database.</param>
        /// <param name="transaction">An active transaction.</param>
        /// <param name="fileFullPath">Fully qualified path to the CSV file.</param>
        /// <param name="delimiter">Delimeter used in CSV file. When NULL, defaults to command ",".</param>
        /// <param name="batchSize"></param>
        /// <param name="commandTimeout"></param>
        void Run(
            IDbConnection connection,
            IDbTransaction transaction,
            string fileFullPath,
            string delimiter = null,
            int? batchSize = null,
            int? commandTimeout = null);
    }
}