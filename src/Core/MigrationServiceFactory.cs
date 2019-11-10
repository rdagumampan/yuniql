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
            else
            {
                throw new System.NotSupportedException($"The target database platform {platform} is not yet supported. See WIKI for supported database platforms.");
            }
        }
    }
}
