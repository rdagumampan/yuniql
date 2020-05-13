using System.Collections.Generic;
using Yuniql.Extensibility;

namespace Yuniql.PlatformTests
{
    public interface ITestDataService
    {
        string GetConnectionString(string databaseName);

        string GetCurrentDbVersion(string connectionString);

        List<DbVersion> GetAllDbVersions(string connectionString);

        bool QuerySingleBool(string connectionString, string sqlStatement);

        string QuerySingleString(string connectionString, string sqlStatement);

        bool CheckIfDbExist(string connectionString);

        bool CheckIfDbObjectExist(string connectionString, string objectName);

        string GetSqlForCreateDbSchema(string schemaName);

        string GetSqlForCreateDbObject(string scriptName);

        string GetSqlForCreateDbObjectWithError(string objectName);

        string GetSqlForCreateDbObjectWithTokens(string objectName);

        string GetSqlForCreateBulkTable(string tableName);

        string GetSqlForSingleLine(string objectName);

        string GetSqlForSingleLineWithoutTerminator(string objectName);

        string GetSqlForMultilineWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3);

        string GetSqlForMultilineWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3);

        string GetSqlForMultilineWithError(string objectName1, string objectName2);

        string GetSqlForMultilineWithTerminatorInCommentBlock(string objectName1, string objectName2, string objectName3);

        void CreateScriptFile(string sqlFilePath, string sqlStatement);

        string GetSqlForCleanup();

        bool IsAtomicDDLSupported { get; }

        bool IsSchemaSupported { get; }

        bool IsBatchSqlSupported { get; }

        string TableName { get;}

        string SchemaName { get; }

        List<BulkTestDataRow> GetBulkTestData(string connectionString, string tableName);
    }
}