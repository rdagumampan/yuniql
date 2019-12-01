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

        public void Run(IDbConnection connection, IDbTransaction transaction, string csvFileFullPath)
        {
            //read csv file and load into data table
            var dataTable = ParseCsvFile(connection, csvFileFullPath);

            //save the csv data into staging sql table
            BulkCopyWithDataTable(connection, transaction, dataTable);
        }

        private DataTable ParseCsvFile(IDbConnection connection, string csvFileFullPath)
        {
            var csvDatatable = new DataTable();
            csvDatatable.TableName = Path.GetFileNameWithoutExtension(csvFileFullPath);

            string query = $"SELECT * FROM " + csvDatatable.TableName + " LIMIT 0;";
            using (var adapter = new MySqlDataAdapter(query, connection as MySqlConnection))
            {
                adapter.Fill(csvDatatable);
            };

            using (var csvReader = new CsvTextFieldParser(csvFileFullPath))
            {
                csvReader.Delimiters = (new string[] { "," });
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
        private void BulkCopyWithDataTable(IDbConnection connection, IDbTransaction transaction, DataTable dataTable)
        {
            _traceService.Info($"MySqlBulkImportService: Started copying data into destination table {dataTable.TableName}");

            using (var cmd = new MySqlCommand())
            {
                cmd.Connection = connection as MySqlConnection;
                cmd.Transaction = transaction as MySqlTransaction;
                cmd.CommandText = $"SELECT * FROM " + dataTable.TableName + " LIMIT 0;";

                using (var adapter = new MySqlDataAdapter(cmd))
                {
                    adapter.UpdateBatchSize = 10000;
                    using (var cb = new MySqlCommandBuilder(adapter))
                    {
                        cb.SetAllValues = true;
                        adapter.Update(dataTable);
                    }
                };

                _traceService.Info($"MySqlBulkImportService: Finished copying data into destination table {dataTable.TableName}");
            }
        }
    }
}
