using System.Collections.Generic;
using Yuniql.Extensibility;

namespace Yuniql.Extensibility
{
    public interface ITestDataService
    {
        bool CheckDbExist(string connectionString);

        string CreateBulkTableScript(string tableName);

        string CreateDbObjectScript(string scriptName);

        string CreateCheckDbObjectExistScript(string objectName);

        string CreateCleanupScript();

        string CreateMultilineScriptWithError(string objectName1, string objectName2);

        string CreateMultilineScriptWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3);

        string CreateMultilineScriptWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3);

        void CreateScriptFile(string sqlFilePath, string sqlStatement);

        string CreateSingleLineScript(string objectName);

        string CreateSingleLineScriptWithoutTerminator(string objectName);

        string CreateTokenizedDbObjectScript(string objectName);

        List<DbVersion> GetAllDbVersions(string connectionString);

        string GetConnectionString(string databaseName);

        string GetCurrentVersion(string connectionString);

        bool QuerySingleBool(string connectionString, string sqlStatement);

        string QuerySingleString(string connectionString, string sqlStatement);
    }
}