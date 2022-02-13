using Snowflake.Data.Client;
using Snowflake.Data.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Yuniql.Extensibility;
using Yuniql.Extensibility.BulkCsvParser;

//https://github.com/22222/CsvTextFieldParser
namespace Yuniql.Snowflake
{
    public class SnowflakeBulkImportService : IBulkImportService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        public SnowflakeBulkImportService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;

            //configure snowflake loggers to follow yuniql debug settings
            var logger = SFLoggerFactory.GetLogger<SnowflakeDataService>();
            logger.SetDebugMode(_traceService.IsDebugEnabled);
        }

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
            var fileNameSegments = fileName.SplitBulkFileName(defaultSchema: "PUBLIC");
            var schemaName = fileNameSegments.Item2;
            var tableName = fileNameSegments.Item3;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _traceService.Info($"SnowflakeImportService: Started copying data into destination table {schemaName}.{tableName}");

            //read csv file and load into data table
            var sqlStatement = PrepareMultiRowInsertStatement(schemaName, tableName, fileFullPath, delimiter);
            var statementCorrelationId = Guid.NewGuid().ToString().Fixed();
            _traceService.Debug($"Executing statement {statementCorrelationId}: {Environment.NewLine}{sqlStatement}");

            using (var cmd = new SnowflakeDbCommand())
            {
                cmd.Connection = connection as SnowflakeDbConnection;
                cmd.Transaction = transaction as SnowflakeDbTransaction;
                cmd.CommandText = sqlStatement;
                cmd.ExecuteNonQuery();
            }

            stopwatch.Stop();
            _traceService?.Debug($"Statement {statementCorrelationId} executed in {stopwatch.ElapsedMilliseconds} ms");
            _traceService.Info($"SnowflakeImportService: Finished copying data into destination table {schemaName}.{tableName} in {stopwatch.ElapsedMilliseconds} ms");
        }

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

                //enclose all column names into double quote for case-sensitivity
                var csvColumns = csvReader.ReadFields().Select(f => f.DoubleQuote());

                sqlStatement.Append($"INSERT INTO {schemaName.DoubleQuote()}.{tableName.DoubleQuote()} ({string.Join(",", csvColumns)}) {Environment.NewLine}");
                sqlStatement.Append("VALUES ");

                while (!csvReader.EndOfData)
                {
                    var fieldData = csvReader.ReadFields().Select(s =>
                    {
                        if (string.IsNullOrEmpty(s) || s == nullValue || s == nullValueDoubleQuoted || s == nullValueQuoted)
                            return nullValue;
                        return s.Quote();
                    });

                    sqlStatement.Append($"  {Environment.NewLine}({string.Join(",", fieldData)}),");
                }
            }
            return sqlStatement
                .ToString()
                .TrimEnd(',') + ";";
        }
    }
}

