using System.Data;
using System.IO;
using Yuniql.Extensibility;
using MySql.Data.MySqlClient;

//https://github.com/22222/CsvTextFieldParser
namespace Yuniql.MySql
{
    public class MySqlBulkImportService : IBulkImportService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        public MySqlBulkImportService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public void Run(
            IDbConnection connection,
            IDbTransaction transaction,
            string fileFullPath,
            string delimiter = null,
            int? batchSize = null,
            int? commandTimeout = null)
        {
            //extract destination table name, mysql is case sensitive!
            var tableName = Path.GetFileNameWithoutExtension(fileFullPath);

            //read csv file and load into data table
            var dataTable = ParseCsvFile(connection, fileFullPath, tableName, delimiter);

            //save the csv data into staging sql table
            BulkCopyWithDataTable(connection, transaction, tableName, dataTable);
        }

        private DataTable ParseCsvFile(
            IDbConnection connection,
            string fileFullPath,
            string tableName,
            string delimiter)
        {
            if (string.IsNullOrEmpty(delimiter))
                delimiter = ",";

            var csvDatatable = new DataTable();
            string query = $"SELECT * FROM {tableName} LIMIT 0;";
            using (var adapter = new MySqlDataAdapter(query, connection as MySqlConnection))
            {
                adapter.Fill(csvDatatable);
            };

            using (var csvReader = new CsvTextFieldParser(fileFullPath))
            {
                csvReader.Delimiters = (new string[] { delimiter });
                csvReader.HasFieldsEnclosedInQuotes = true;

                //skipped the first row
                csvReader.ReadFields();

                //process data rows
                while (!csvReader.EndOfData)
                {
                    string[] fieldData = csvReader.ReadFields();
                    for (int i = 0; i < fieldData.Length; i++)
                    {
                        if (fieldData[i] == "")
                        {
                            fieldData[i] = null;
                        }
                    }
                    csvDatatable.Rows.Add(fieldData);
                }
            }
            return csvDatatable;
        }

        //https://dev.mysql.com/doc/connector-net/en/connector-net-programming-bulk-loader.html
        //https://stackoverflow.com/questions/48018614/insert-datatable-into-a-mysql-table-using-c-sharp

        //NOTE: This is not the most typesafe and performant way to do this and this is just to demonstrate
        //possibility to bulk import data in custom means during migration execution
        private void BulkCopyWithDataTable(
            IDbConnection connection, 
            IDbTransaction transaction,
            string tableName,
            DataTable dataTable)
        {
            _traceService.Info($"MySqlBulkImportService: Started copying data into destination table {tableName}");

            using (var cmd = new MySqlCommand())
            {
                cmd.Connection = connection as MySqlConnection;
                cmd.Transaction = transaction as MySqlTransaction;
                cmd.CommandText = $"SELECT * FROM {tableName} LIMIT 0;";

                using (var adapter = new MySqlDataAdapter(cmd))
                {
                    adapter.UpdateBatchSize = 10000;
                    using (var cb = new MySqlCommandBuilder(adapter))
                    {
                        cb.SetAllValues = true;
                        adapter.Update(dataTable);
                    }
                };

                _traceService.Info($"MySqlBulkImportService: Finished copying data into destination table {tableName}");
            }
        }
    }
}
