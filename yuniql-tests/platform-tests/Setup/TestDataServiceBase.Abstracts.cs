using Yuniql.PlatformTests.Interfaces;

namespace Yuniql.PlatformTests.Setup
{
    public abstract partial class TestDataServiceBase : ITestDataService
    {
        public abstract string GetConnectionString(string databaseName);

        public abstract bool CheckIfDbExist(string connectionString);

        public abstract bool CheckIfDbObjectExist(string connectionString, string objectName);

        public abstract void CreateScriptFile(string sqlFilePath, string sqlStatement);

        public abstract string GetSqlForEraseDbObjects();

        public abstract string GetSqlForCreateBulkTable(string tableName);

        public abstract string GetSqlForCreateDbObject(string scriptName);

        public abstract string GetSqlForCreateDbObjectWithError(string objectName);

        public abstract string GetSqlForCreateDbObjectWithTokens(string objectName);

        public abstract string GetSqlForCreateDbSchema(string schemaName);

        public abstract string GetSqlForMultilineWithError(string objectName1, string objectName2);

        public abstract string GetSqlForMultilineWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3);

        public abstract string GetSqlForMultilineWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3);

        public abstract string GetSqlForSingleLine(string objectName);

        public abstract string GetSqlForSingleLineWithoutTerminator(string objectName);

        public abstract string GetSqlForMultilineWithTerminatorInCommentBlock(string objectName1, string objectName2, string objectName3);

        public abstract string GetSqlForGetBulkTestData(string tableName);

        public abstract void CleanupDbObjects(string connectionString);
 
    }
}
