using System.Data.SqlClient;
using System.Data;
using System.IO;
using Yuniql.Extensibility;

//https://github.com/22222/CsvTextFieldParser
namespace Yuniql.PostgreSql
{
    public class PostgreSqlCsvImportService : ICsvImportService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        public PostgreSqlCsvImportService(ITraceService traceService)
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
            var dataTable = ParseCsvFile(csvFileFullPath);

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
            //var uploader = new NpgsqlBulkUploader(context);
            //var data = GetALotOfData();
            //uploader.Insert(data);
        }
    }
}

