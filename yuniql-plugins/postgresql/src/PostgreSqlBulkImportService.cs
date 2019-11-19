using System.Data;
using System.IO;
using Yuniql.Extensibility;
using NpgsqlTypes;
using System.Collections.Generic;
using Npgsql;
using System;

//https://github.com/22222/CsvTextFieldParser
namespace Yuniql.PostgreSql
{
    public class PostgreSqlBulkImportService : IBulkImportService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        public PostgreSqlBulkImportService(ITraceService traceService)
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

        //NOTE: This is not the most typesafe and performant way to do this and this is just to demonstrate
        //possibility to bulk import data in custom means during migration execution
        //https://www.npgsql.org/doc/copy.html
        private void BulkCopyWithDataTable(IDbConnection connection, IDbTransaction transaction, DataTable dataTable)
        {
            //remove the first row as its the column names
            dataTable.Rows[0].Delete();

            //prepare list of columns in the target table
            var columnNames = new List<string>();
            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                columnNames.Add(dataColumn.ColumnName);
            }

            var sqlStatement = $"COPY {dataTable.TableName} ({string.Join(',', columnNames.ToArray())}) FROM STDIN (FORMAT BINARY)";
            _traceService.Info("PostgreSqlBulkImportService: " + sqlStatement);

            var pgsqlConnection = connection as Npgsql.NpgsqlConnection;
            using (var writer = pgsqlConnection.BeginBinaryImport(sqlStatement))
            {
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    writer.StartRow();
                    foreach (DataColumn dataColumn in dataTable.Columns)
                    {
                        _traceService.Info(dataRow[dataColumn.ColumnName].ToString());

                        if (dataColumn.ColumnName != "BirthDate")
                        {
                            writer.Write(dataRow[dataColumn.ColumnName].ToString());
                        }
                        else
                        {
                            writer.Write(DateTime.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.Timestamp);
                        }
                    }
                }

                writer.Complete();
            }
        }
        private List<ColumnDefinition> GetDestinationSchema(string tableName)
        {
            var result = new List<ColumnDefinition>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"SELECT column_name, data_type FROM information_schema.COLUMNS WHERE TABLE_NAME = '{tableName}'";
                command.CommandTimeout = 0;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ColumnDefinition
                        {
                            DbColumnName = reader.GetString(0),
                            DbDataType = reader.GetString(1)
                        });
                    }
                }
            }

            return result;
        }
    }

    public class ColumnDefinition
    {
        public string DbColumnName { get; set; }
        public string DbDataType { get; set; }
    }
}

