using System.Data;

namespace Yuniql.Extensibility
{
    public interface ICsvImportService
    {
        void Initialize(string connectionString);

        void Run(IDbConnection connection, IDbTransaction transaction, string csvFileFullPath);
    }
}