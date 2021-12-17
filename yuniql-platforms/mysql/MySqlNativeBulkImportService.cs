using System.Data;
using System.IO;
using Yuniql.Extensibility;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace Yuniql.MySql
{
    ///<inheritdoc/>
    public class MySqlNativeBulkImportService : IBulkImportService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public MySqlNativeBulkImportService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        ///<inheritdoc/>
        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        //https://wiki.ispirer.com/sqlways/troubleshooting-guide/mysql/import/command-not-allowed-data-load
        //https://www.youtube.com/watch?v=XM2xx-PD4cg&vl=en
        //https://dev.mysql.com/doc/connector-net/en/connector-net-programming-bulk-loader.html
        ///<inheritdoc/>
        public void Run(
            IDbConnection connection,
            IDbTransaction transaction,
            string fileFullPath,
            string bulkSeparator = null,
            int? bulkBatchSize = null,
            int? commandTimeout = null,
            List<KeyValuePair<string, string>> tokens = null
        )
        {
            var tableName = Path.GetFileNameWithoutExtension(fileFullPath);

            _traceService.Info($"MySqlBulkImportService: Started copying data into destination table {tableName}");

            var bulkLoader = new MySqlBulkLoader(connection as MySqlConnection);
            bulkLoader.Local = true;
            bulkLoader.TableName = tableName;
            bulkLoader.FieldTerminator = bulkSeparator;
            bulkLoader.LineTerminator = "\n";
            bulkLoader.FileName = fileFullPath;
            bulkLoader.NumberOfLinesToSkip = 1;

            int affectedRecords = bulkLoader.Load();

            _traceService.Info($"MySqlBulkImportService: Finished copying data into destination table {tableName}. {affectedRecords} rows imported.");

        }
    }
}

