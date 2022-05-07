using Microsoft.Data.SqlClient;
using System.Data;
using System.IO;
using Yuniql.Extensibility;
using Yuniql.Extensibility.BulkCsvParser;
using System.Diagnostics;
using System.Collections.Generic;
using System;

//https://github.com/22222/CsvTextFieldParser
namespace Yuniql.SqlServer
{
    ///<inheritdoc/>
    public class SqlServerBulkImportService : IBulkImportService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public SqlServerBulkImportService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        ///<inheritdoc/>
        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

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
            //get file name segments from potentially sequenceno.schemaname.tablename filename pattern
            var fileName = Path.GetFileNameWithoutExtension(fileFullPath)
                          .ReplaceTokens(_traceService, tokens);
            var fileNameSegments = fileName.SplitBulkFileName(defaultSchema: "dbo");
            var schemaName = fileNameSegments.Item2;
            var tableName = fileNameSegments.Item3;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _traceService.Info($"SqlServerBulkImportService: Started copying data into destination table {schemaName}.{tableName}");

            //read csv file and load into data table
            var columnTypes = GetDestinationSchema(tableName, schemaName, connection as SqlConnection, transaction as SqlTransaction);
            var dataTable = ParseCsvFile(fileFullPath, columnTypes, bulkSeparator);

            //save the csv data into staging sql table
            BulkCopyWithDataTable(
                connection,
                transaction,
                schemaName,
                tableName,
                dataTable,
                bulkBatchSize,
                commandTimeout);

            stopwatch.Stop();
            _traceService.Info($"SqlServerBulkImportService: Finished copying data into destination table {schemaName}.{tableName} in {stopwatch.ElapsedMilliseconds} ms");

        }

        private DataTable ParseCsvFile(
            string csvFileFullPath,
            Dictionary<string, DbTypeMap> columnTypes,
            string bulkSeparator = null)
        {
            if (string.IsNullOrEmpty(bulkSeparator))
                bulkSeparator = ",";

            var csvDatatable = new DataTable();
            using (var csvReader = new CsvTextFieldParser(csvFileFullPath))
            {
                //csv reader configuration
                csvReader.Separators = (new string[] { bulkSeparator });

                //configure destination data table
                var csvColumns = csvReader.ReadFields();
                foreach (var csvColumn in csvColumns)
                {
                    var dataColumn = new DataColumn(csvColumn);
                    dataColumn.AllowDBNull = true;
                    if (columnTypes.TryGetValue(csvColumn, out var type))
                    {
                        dataColumn.DataType = type.DotnetType;
                    }

                    csvDatatable.Columns.Add(dataColumn);
                }

                //load up data into data table
                while (!csvReader.EndOfData)
                {
                    string[] fieldData = csvReader.ReadFields();
                    for (int i = 0; i < fieldData.Length; i++)
                    {
                        if (fieldData[i] == "" || fieldData[i] == "NULL")
                        {
                            fieldData[i] = null;
                        }
                    }
                    csvDatatable.Rows.Add(fieldData);
                }
            }

            return csvDatatable;
        }

        private void BulkCopyWithDataTable(
            IDbConnection connection,
            IDbTransaction transaction,
            string schemaName,
            string tableName,
            DataTable dataTable,
            int? bulkBatchSize = null,
            int? commandTimeout = null)
        {
            using (var sqlBulkCopy = new SqlBulkCopy(connection as SqlConnection, SqlBulkCopyOptions.Default, transaction as SqlTransaction))
            {
                sqlBulkCopy.DestinationTableName = $"[{schemaName}].[{tableName}]";
                sqlBulkCopy.BulkCopyTimeout = commandTimeout.HasValue ? commandTimeout.Value : DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS;
                sqlBulkCopy.BatchSize = bulkBatchSize.HasValue ? bulkBatchSize.Value : DEFAULT_CONSTANTS.BULK_BATCH_SIZE;
                sqlBulkCopy.EnableStreaming = true;
                sqlBulkCopy.SqlRowsCopied += SqlBulkCopy_SqlRowsCopied;
                foreach (var column in dataTable.Columns)
                {
                    sqlBulkCopy.ColumnMappings.Add(column.ToString(), column.ToString());
                }
                sqlBulkCopy.WriteToServer(dataTable);
            }
        }

        private void SqlBulkCopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            _traceService.Info($"SqlServerBulkImportService copied {e.RowsCopied} rows");
        }

        private Dictionary<string, DbTypeMap> GetDestinationSchema(string tableName, string schemaName, SqlConnection connection, SqlTransaction transaction)
        {
            var types = new Dictionary<string, DbTypeMap>();
            var sql = $"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.columns c where c.TABLE_NAME = '{tableName}'";

            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var dbTypeMap = new DbTypeMap
                    {
                        ColumnName = reader.GetString(0),
                        SqlTypeName = reader.GetString(1)
                    };

                    var dotnetType = MapSqlTypeToNative(dbTypeMap.SqlTypeName);
                    if (dotnetType == null)
                    {
                        //not supported types: xml, rowversion, sql_variant, image, varbinary(max), binary, varbinary, timestamp
                        throw new NotSupportedException($"SqlServerBulkImportService: Data type '{dbTypeMap.SqlTypeName}' on destination table {schemaName}.{tableName} is not support for bulk import operations.");
                    }
                    else
                    {
                        dbTypeMap.DotnetType = dotnetType;
                        types.Add(dbTypeMap.ColumnName, dbTypeMap);
                    }
                }
            }

            return types;
        }

        private Type MapSqlTypeToNative(string dataType)
        {
            if (dataType == "char" || dataType == "nchar" || dataType == "text" || dataType == "ntext" || dataType == "varchar" || dataType == "nvarchar")
            {
                return typeof(string);
            }
            else if (dataType == "bit")
            {
                return typeof(bool);
            }
            else if (dataType == "int")
            {
                return typeof(Int32);
            }
            else if (dataType == "bigint")
            {
                return typeof(Int64);
            }
            else if (dataType == "smallint")
            {
                return typeof(Int16);
            }
            else if (dataType == "uniqueidentifier")
            {
                return typeof(Guid);
            }
            else if (dataType == "varbinary")
            {
                return typeof(Int16);
            }
            else if (dataType == "date" || dataType == "datetime" || dataType == "datetime2" || dataType == "smalldatetime")
            {
                return typeof(DateTime);
            }
            else if (dataType == "datetimeoffset")
            {
                return typeof(DateTimeOffset);
            }
            else if (dataType == "decimal" || dataType == "money" || dataType == "smallmoney" || dataType == "numeric")
            {
                return typeof(Decimal);
            }
            else if (dataType == "time")
            {
                return typeof(TimeSpan);
            }
            else if (dataType == "real")
            {
                return typeof(Single);
            }
            else if (dataType == "float")
            {
                return typeof(Double);
            }
            return null;
        }
    }
}

