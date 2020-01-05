using System.Data;

namespace Yuniql.Extensibility
{
    public interface IBulkImportService
    {
        void Initialize(string connectionString, int commandTimeout = 30);

        void Run(IDbConnection activeConnection, IDbTransaction activeTransaction, string fileFullPath, string delimiter, int batchSize = 0, int commandTimeout = 30);
    }
}