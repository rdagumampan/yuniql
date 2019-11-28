using System.Data;
using System.IO;
using Yuniql.Extensibility;
using System.Collections.Generic;
using MySql.Data;
using System;
using System.Linq;
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
            var dataTable = ParseCsvFile(csvFileFullPath);

            //save the csv data into staging sql table
            BulkCopyWithDataTable(connection, transaction, dataTable);
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

        //https://dev.mysql.com/doc/connector-net/en/connector-net-programming-bulk-loader.html

        //NOTE: This is not the most typesafe and performant way to do this and this is just to demonstrate
        //possibility to bulk import data in custom means during migration execution
        //https://www.npgsql.org/doc/copy.html
        private void BulkCopyWithDataTable(IDbConnection connection, IDbTransaction transaction, DataTable dataTable)
        {
            _traceService.Info($"MySqlBulkImportService: Started copying data into destination table {dataTable.TableName}");

            //remove the first row as its the column names
            dataTable.Rows[0].Delete();

            //var bulkLoader = new MySqlBulkLoader(connection as MySqlConnection);
            //bulkLoader.TableName = dataTable.TableName;
            
            _traceService.Info($"MySqlBulkImportService: Finished copying data into destination table {dataTable.TableName}");
        }

        //https://www.npgsql.org/doc/types/basic.html
        private IDictionary<string, ColumnDefinition> GetDestinationSchema(string tableName)
        {
            var result = new Dictionary<string, ColumnDefinition>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"SELECT column_name, data_type FROM information_schema.COLUMNS WHERE TABLE_NAME = '{tableName.ToLower()}'";
                command.CommandTimeout = 0;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0), new ColumnDefinition
                        {
                            ColumnName = reader.GetString(0),
                            DataType = reader.GetString(1)
                        });
                    }
                }
            }

            return result;
        }
    }

    public class ColumnDefinition
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
    }
}

