using System.Data;
using System.IO;
using Yuniql.Extensibility;
using Oracle.ManagedDataAccess.Client;
using Yuniql.Extensibility.BulkCsvParser;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Linq;

//https://github.com/22222/CsvTextFieldParser
namespace Yuniql.Oracle
{
    ///<inheritdoc/>
    public class OracleBulkImportService : IBulkImportService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public OracleBulkImportService(ITraceService traceService)
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
                   string delimiter = null,
                   int? batchSize = null,
                   int? commandTimeout = null,
                   List<KeyValuePair<string, string>> tokens = null
               )
        {
            //get file name segments from potentially sequenceno.schemaname.tablename filename pattern
            var fileName = Path.GetFileNameWithoutExtension(fileFullPath)
                          .ReplaceTokens(_traceService, tokens);
            var fileNameSegments = fileName.SplitBulkFileName(defaultSchema: "");
            var schemaName = fileNameSegments.Item2.HasLower() ? fileNameSegments.Item2.DoubleQuote() : fileNameSegments.Item2;
            var tableName = fileNameSegments.Item3.HasLower() ? fileNameSegments.Item3.DoubleQuote() : fileNameSegments.Item3;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _traceService.Info($"OracleBulkImportService: Started copying data into destination table {tableName}");

            //read csv file and load into data table
            var sqlStatement = PrepareMultiRowInsertStatement(schemaName, tableName, fileFullPath, delimiter);
            var statementCorrelationId = Guid.NewGuid().ToString().Fixed();
            _traceService.Debug($"Executing statement {statementCorrelationId}: {Environment.NewLine}{sqlStatement}");

            using (var cmd = new OracleCommand())
            {
                cmd.Connection = connection as OracleConnection;
                cmd.Transaction = transaction as OracleTransaction;
                cmd.CommandText = sqlStatement;
                cmd.ExecuteNonQuery();
            }

            stopwatch.Stop();
            _traceService?.Debug($"Statement {statementCorrelationId} executed in {stopwatch.ElapsedMilliseconds} ms");
            _traceService.Info($"OracleBulkImportService: Finished copying data into destination table {tableName} in {stopwatch.ElapsedMilliseconds} ms");
        }

        //https://stackoverflow.com/questions/39576/best-way-to-do-multi-row-insert-in-oracle
        //https://stackoverflow.com/questions/28523262/multiple-insert-sql-oracle
        //NOTE: This is not the most typesafe and performant way to do this and this is just to demonstrate
        //possibility to bulk import data in custom means during migration execution
        private string PrepareMultiRowInsertStatement(
            string schemaName,
            string tableName,
            string csvFileFullPath,
            string delimiter = null)
        {
            var sqlStatement = new StringBuilder();
            var nullValue = "NULL";
            var nullValueDoubleQuoted = "NULL".DoubleQuote();
            var nullValueQuoted = "NULL".Quote();

            if (string.IsNullOrEmpty(delimiter))
                delimiter = ",";

            //prepare local constants for optimal conditional evaluation
            using (var csvReader = new CsvTextFieldParser(csvFileFullPath))
            {
                csvReader.Separators = (new string[] { delimiter });

                var csvColumns = csvReader.ReadFields().Select(f => f);
                sqlStatement.Append($"INSERT ALL");
                while (!csvReader.EndOfData)
                {
                    var fieldData = csvReader.ReadFields().Select(fv =>
                    {
                        if (string.IsNullOrEmpty(fv) || fv == nullValue || fv == nullValueDoubleQuoted || fv == nullValueQuoted)
                            return nullValue;
                        return fv.Quote();
                    });

                    sqlStatement.Append($"{Environment.NewLine}INTO {tableName} ({string.Join(",", csvColumns)}) VALUES ({string.Join(",", fieldData)})");
                }
            }

            return sqlStatement
                .Append($"{Environment.NewLine}SELECT 1 FROM DUAL")  //required for INSERT ALL to work
                .ToString();
        }
    }
}
