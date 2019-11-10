using ArdiLabs.Yuniql.SqlServer;

namespace ArdiLabs.Yuniql
{
    public class MigrationServiceFactory : IMigrationServiceFactory
    {

        public IMigrationService Create(string platform)
        {
            if (platform.Equals("sqlserver"))
            {
                var dataService = new SqlServerDataService();
                var csvImportService = new SqlServerCsvImportService();

                var migrationService = new MigrationService(dataService, csvImportService);
                return migrationService;
            }

            return null;
        }
    }
}
