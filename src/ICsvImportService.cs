using System.Data.SqlClient;

namespace ArdiLabs.Yuniql
{
    public interface ICsvImportService
    {
        void Run(SqlConnectionStringBuilder sqlConnectionString, string csvFileFullPath);
    }
}