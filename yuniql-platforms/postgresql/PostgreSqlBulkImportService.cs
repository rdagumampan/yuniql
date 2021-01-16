using System.Data;
using System.IO;
using Yuniql.Extensibility;
using NpgsqlTypes;
using System.Collections.Generic;
using Npgsql;
using System;
using System.Linq;
using Yuniql.Extensibility.BulkCsvParser;

//https://github.com/22222/CsvTextFieldParser
namespace Yuniql.PostgreSql
{
    ///<inheritdoc/>
    public class PostgreSqlBulkImportService : IBulkImportService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public PostgreSqlBulkImportService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        ///<inheritdoc/>
        public void Initialize(
            string connectionString)
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
            int? commandTimeout = null)
        {
            //get file name segments from potentially sequenceno.schemaname.tablename filename pattern
            //assumes all objects are not double quoted because pgsql auto-lower case all undouble quoted objects
            var fileName = Path.GetFileNameWithoutExtension(fileFullPath);
            var fileNameSegments = fileName.SplitBulkFileName(defaultSchema: "public");
            var schemaName = fileNameSegments.Item2.IsDoubleQuoted() ? fileNameSegments.Item2 : fileNameSegments.Item2.ToLower();
            var tableName = fileNameSegments.Item3.IsDoubleQuoted() ? fileNameSegments.Item3 : fileNameSegments.Item3.ToLower();   

            //read csv file and load into data table
            var dataTable = ParseCsvFile(fileFullPath, bulkSeparator);

            //save the csv data into staging sql table
            BulkCopyWithDataTable(connection, transaction, schemaName, tableName, dataTable);
        }

        private DataTable ParseCsvFile(string csvFileFullPath, string bulkSeparator)
        {
            if (string.IsNullOrEmpty(bulkSeparator))
                bulkSeparator = ",";

            var csvDatatable = new DataTable();
            using (var csvReader = new CsvTextFieldParser(csvFileFullPath))
            {
                csvReader.Separators = (new string[] { bulkSeparator });
                csvReader.HasFieldsEnclosedInQuotes = true;

                string[] csvColumns = csvReader.ReadFields();
                foreach (string csvColumn in csvColumns)
                {
                    var dataColumn = new DataColumn(csvColumn.ToLower());
                    dataColumn.AllowDBNull = true;
                    csvDatatable.Columns.Add(dataColumn);
                }

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

        //NOTE: This is not the most typesafe and performant way to do this and this is just to demonstrate
        //possibility to bulk import data in custom means during migration execution
        //https://www.npgsql.org/doc/copy.html
        private void BulkCopyWithDataTable(
            IDbConnection connection,
            IDbTransaction transaction,
            string schemaName,
            string tableName,
            DataTable dataTable)
        {
            _traceService.Info($"PostgreSqlBulkImportService: Started copying data into destination table {schemaName}.{tableName}");

            //get destination table schema and filter out columns not in csv file
            var destinationSchema = GetDestinationSchema(schemaName, tableName);
            var destinationColumns = destinationSchema.ToList().Where(f => dataTable.Columns.Contains(f.Key)).Select(k => k.Key).ToArray();

            //prepare statement for binary import
            var sqlStatement = $"COPY {schemaName}.{tableName} ({string.Join(',', destinationColumns)}) FROM STDIN (FORMAT BINARY)";
            _traceService.Info("PostgreSqlBulkImportService: " + sqlStatement);

            var pgsqlConnection = connection as NpgsqlConnection;
            using (var writer = pgsqlConnection.BeginBinaryImport(sqlStatement))
            {
                //writes each data row as datastream into pgsql database
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    writer.StartRow();
                    foreach (DataColumn dataColumn in dataTable.Columns)
                    {
                        if (dataRow.IsNull(dataColumn.ColumnName))
                        {
                            writer.Write(DBNull.Value);
                            continue;
                        }

                        if (!destinationSchema.ContainsKey(dataColumn.ColumnName))
                            continue;

                        var dataType = destinationSchema[dataColumn.ColumnName.ToLower()].DataType;

                        if (dataType == "boolean" || dataType == "bit" || dataType == "bit varying")
                        {
                            writer.Write(bool.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.Boolean);
                            continue;
                        }
                        else if (dataType == "smallint" || dataType == "int2")
                        {
                            writer.Write(short.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.Smallint);
                            continue;
                        }
                        else if (dataType == "integer" || dataType == "int4")
                        {
                            writer.Write(int.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.Integer);
                            continue;
                        }
                        else if (dataType == "bigint" || dataType == "int8")
                        {
                            writer.Write(long.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.Bigint);
                            continue;
                        }
                        else if (dataType == "real")
                        {
                            writer.Write(float.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.Real);
                            continue;
                        }
                        else if (dataType == "double precision")
                        {
                            writer.Write(double.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.Double);
                            continue;
                        }
                        else if (dataType == "numeric" || dataType == "money")
                        {
                            writer.Write(decimal.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.Numeric);
                            continue;
                        }
                        else if (dataType == "uuid")
                        {
                            writer.Write(Guid.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.Uuid);
                            continue;
                        }
                        else if (dataType == "date")
                        {
                            writer.Write(DateTime.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.Date);
                            continue;
                        }
                        else if (dataType == "interval")
                        {
                            writer.Write(TimeSpan.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.Interval);
                            continue;
                        }
                        else if (dataType == "timestamp" || dataType == "timestamp without time zone")
                        {
                            writer.Write(DateTime.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.Timestamp);
                            continue;
                        }
                        else if (dataType == "timestamp with time zone")
                        {
                            writer.Write(DateTimeOffset.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.TimestampTz);
                            continue;
                        }
                        else if (dataType == "time" || dataType == "time without time zone")
                        {
                            writer.Write(DateTime.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.Time);
                            continue;
                        }
                        else if (dataType == "time with time zone")
                        {
                            writer.Write(DateTime.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.TimeTz);
                            continue;
                        }
                        else if (dataType == "name")
                        {
                            writer.Write(dataRow[dataColumn.ColumnName].ToString(), NpgsqlDbType.Name);
                            continue;
                        }
                        else if (dataType == "(internal) char")
                        {
                            writer.Write(byte.Parse(dataRow[dataColumn.ColumnName].ToString()), NpgsqlDbType.InternalChar);
                            continue;
                        }
                        else if (dataType == "text"
                            || dataType == "character varying"
                            || dataType == "character"
                            || dataType == "citext"
                            || dataType == "json"
                            || dataType == "jsonb"
                            || dataType == "xml")
                        {
                            writer.Write(dataRow[dataColumn.ColumnName].ToString());
                            continue;
                        }
                        else
                        {
                            //not supported types: lseg,path,polygon,line,circle,box,hstore,cidr,inet,macaddr,tsquery,tsvector,bytea,oid,xid,cid,oidvector,composite types,range types,enum types,array types
                            throw new NotSupportedException($"PostgreSqlBulkImportService: Data type '{dataType}' on destination table {schemaName}.{tableName} is not support for bulk import operations.");
                        }
                    }
                }

                //wraps up everything, closes the stream
                writer.Complete();
            }

            _traceService.Info($"PostgreSqlBulkImportService: Finished copying data into destination table {tableName}");
        }

        //https://www.npgsql.org/doc/types/basic.html
        private IDictionary<string, ColumnDefinition> GetDestinationSchema(string schemaName, string tableName)
        {
            var result = new Dictionary<string, ColumnDefinition>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"SELECT column_name, data_type FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = '{schemaName}' AND TABLE_NAME = '{tableName}'";
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

