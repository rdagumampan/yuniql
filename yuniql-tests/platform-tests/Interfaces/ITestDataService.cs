using System.Collections.Generic;
using Yuniql.Extensibility;
using Yuniql.PlatformTests.Setup;

namespace Yuniql.PlatformTests.Interfaces
{
    public interface ITestDataService
    {
        bool IsMultiTenancySupported { get; }

        bool IsSchemaSupported { get; }

        bool IsTransactionalDdlSupported { get; }

        bool IsBatchSqlSupported { get; }

        string MetaTableName { get; }

        string MetaSchemaName { get; }

        string GetConnectionString(string databaseName);

        List<string> BreakStatements(string sqlStatement);

        string GetCurrentDbVersion(string connectionString);

        List<DbVersion> GetAllDbVersions(string connectionString);

        bool QuerySingleBool(string connectionString, string sqlStatement);

        string QuerySingleString(string connectionString, string sqlStatement);

        bool QuerySingleRow(string connectionString, string sqlStatement);

        string GetSqlForCreateDbSchema(string schemaName);

        bool CheckIfDbExist(string connectionString);

        bool CheckIfDbObjectExist(string connectionString, string objectName);

        string GetSqlForCreateDbObject(string objectName);

        string GetSqlForCreateDbObjectWithError(string objectName);

        string GetSqlForCreateDbObjectWithTokens(string objectName);

        string GetSqlForCreateBulkTable(string objectName);

        string GetSqlForSingleLine(string objectName);

        string GetSqlForMultilineWithTerminatorInCommentBlock(string objectName1, string objectName2, string objectName3);

        string GetSqlForMultilineWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3);

        string GetSqlForSingleLineWithoutTerminator(string objectName);

        string GetSqlForMultilineWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3);

        string GetSqlForMultilineWithError(string objectName1, string objectName2);

        void CreateScriptFile(string sqlFilePath, string sqlStatement);

        string GetSqlForEraseDbObjects();

        void CleanupDbObjects(string connectionString);

        List<BulkTestDataRow> GetBulkTestData(string connectionString, string objectName);
    }
}