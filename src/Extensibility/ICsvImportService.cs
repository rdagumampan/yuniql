using System.Data;

namespace ArdiLabs.Yuniql
{
    public interface ICsvImportService
    {
        void Initialize(string connectionString);

        void Run(IDbConnection connection, IDbTransaction transaction, string csvFileFullPath);
    }
}