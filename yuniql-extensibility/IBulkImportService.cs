using System.Data;

namespace Yuniql.Extensibility
{
    public interface IBulkImportService
    {
        void Initialize(
            string connectionString);

        void Run(
            IDbConnection connection,
            IDbTransaction transaction,
            string fileFullPath,
            string delimiter = null,
            int? batchSize = null,
            int? commandTimeout = null);
    }
}