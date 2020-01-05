using System.Data;

namespace Yuniql.Extensibility
{
    public interface IBulkImportService
    {
        void Initialize(string connectionString, int commandTimeout = DefaultConstants.CommandTimeoutSecs);

        void Run(IDbConnection connection, IDbTransaction transaction, string fileFullPath, string delimiter, int batchSize = 0, int commandTimeout = DefaultConstants.CommandTimeoutSecs);
    }
}