using System.Data.SqlClient;
using System.Data;
using System.IO;
using Yuniql.Extensibility;

//https://github.com/22222/CsvTextFieldParser
namespace Yuniql.SqlServer
{
    public class SqlServerBulkImportService : IBulkImportService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        public SqlServerBulkImportService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public void Run(IDbConnection connection, IDbTransaction transaction, string fileFullPath)
        {
            //read csv file and load into data table
            var dataTable = ParseCsvFile(fileFullPath);

            //save the csv data into staging sql table
            BulkCopyWithDataTable(connection, transaction, dataTable);

            //TODO: validate staging data against destination table schema defs

            //TODO: transport staging data into destination table
        }

        private DataTable ParseCsvFile(string csvFileFullPath)
        {
            var csvDatatable = new DataTable();
            csvDatatable.TableName = Path.GetFileNameWithoutExtension(csvFileFullPath);

            using (var csvReader = new CsvTextFieldParser(csvFileFullPath))
            {
                csvReader.Delimiters = (new string[] { "," });
                csvReader.HasFieldsEnclosedInQuotes = true;

                string[] csvColumns = csvReader.ReadFields();
                foreach (string csvColumn in csvColumns)
                {
                    var dataColumn = new DataColumn(csvColumn);
                    dataColumn.AllowDBNull = true;
                    csvDatatable.Columns.Add(dataColumn);
                }

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

        private void BulkCopyWithDataTable(IDbConnection connection, IDbTransaction transaction, DataTable csvFileDatatTable)
        {
            using (var sqlBulkCopy = new SqlBulkCopy(connection as SqlConnection, SqlBulkCopyOptions.Default, transaction as SqlTransaction))
            {
                sqlBulkCopy.DestinationTableName = csvFileDatatTable.TableName;
                sqlBulkCopy.BatchSize = 0;
                sqlBulkCopy.EnableStreaming = true;
                sqlBulkCopy.SqlRowsCopied += SqlBulkCopy_SqlRowsCopied;
                foreach (var column in csvFileDatatTable.Columns)
                {
                    sqlBulkCopy.ColumnMappings.Add(column.ToString(), column.ToString());
                }
                sqlBulkCopy.WriteToServer(csvFileDatatTable);
            }
        }
        
        private void SqlBulkCopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            _traceService.Info($"SqlServerBulkImportService copied {e.RowsCopied} rows");
        }
    }
}

