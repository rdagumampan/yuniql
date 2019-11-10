using ArdiLabs.Yuniql.SqlServer;

namespace ArdiLabs.Yuniql
{
    public class CsvImportServiceFactory : ICsvImportServiceFactory
    {
        public ICsvImportService Create(string platform)
        {
            if (platform.Equals("sqlserver"))
            {
                return new SqlServerCsvImportService();
            }

            return null;
        }
    }
}
