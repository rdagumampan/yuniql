using System.Collections.Generic;

namespace Yuniql.Extensibility
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

        string CreateDbObjectScript(string scriptName);

        string CreateDbObjectScriptWithError(string objectName);

        string CreateTokenizedDbObjectScript(string objectName);

        string CreateBulkTableScript(string tableName);

        string CreateSingleLineScript(string objectName);

        string CreateSingleLineScriptWithoutTerminator(string objectName);

        string CreateMultilineScriptWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3);

        string CreateMultilineScriptWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3);

        string CreateMultilineScriptWithError(string objectName1, string objectName2);

        void CreateScriptFile(string sqlFilePath, string sqlStatement);

        string CreateCleanupScript();

        bool IsAtomicDDLSupported { get; }

        public bool DropTestDatabase(string connectionString, string databaseName);

        List<BulkTestDataRow> GetBulkTestData(string connectionString, string tableName);
    }
}