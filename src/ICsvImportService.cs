using System.Data;
using System.Data.SqlClient;

namespace ArdiLabs.Yuniql
{
    public interface ICsvImportService
    {
        void Run(IDbConnection connection, IDbTransaction transaction, string csvFileFullPath);
    }
}