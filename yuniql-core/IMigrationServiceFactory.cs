using Yuniql.Extensibility;

namespace Yuniql.Core
{
    /// <summary>
    /// Factory class of creating instance of <see cref="IMigrationService"/>.
    /// </summary>
    public interface IMigrationServiceFactory
    {
        /// <summary>
        /// Create instance of <see cref="IMigrationService"/> and uses external data services.
        /// When targeting PostgreSql or MySql, this is where you can pass the implementation of <see cref="IDataService"/> and <see cref="IBulkImportService"/>.
        /// </summary>
        /// <param name="dataService">Platform specific data service providing compatible SQL statements and connection objects.</param>
        /// <param name="bulkImportService">Platform specific service provding support for bulk import of CSV files.</param>
        /// <returns>An instance of <see cref="IMigrationService"/> and uses external data services.</returns>
        IMigrationService Create(IDataService dataService, IBulkImportService bulkImportService);
    }
}