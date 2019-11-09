using System.Data;

namespace ArdiLabs.Yuniql
{
    public interface ICsvImportService
    {
        void Run(IDbConnection connection, IDbTransaction transaction, string csvFileFullPath);
    }
}