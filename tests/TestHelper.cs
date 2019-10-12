using System;
using System.IO;

namespace Yuniql.Tests
{
    public static class TestHelper
    {
        public static string GetConnectionString(string databaseName)
        {
            //use this when running against local instance of sql server with integrated security
            //return $"Data Source=.;Integrated Security=SSPI;Initial Catalog={databaseName}";

            //use this when running against sql server container with published port 1401
            return $"Server=localhost,1401;Database={databaseName};User Id=sa;Password=P@ssw0rd!";
        }

        public static string GetWorkingPath()
        {
            return Path.Combine(Environment.CurrentDirectory, @$"yuniqltests_{Guid.NewGuid().ToString().Substring(0, 4)}");
        }

        public static void CleanUp(string workingPath)
        {
            if (Directory.Exists(workingPath))
            {
                Directory.Delete(workingPath, true);
            }
        }
        public static string CreateScript(string scriptName)
        {
            return $@"
                CREATE PROC [dbo].[{scriptName}]
                AS
                    SELECT 1;
                GO
                ";
        }

        public static string CreateAssetScript(string objectName)
        {
            return $"SELECT ISNULL(OBJECT_ID('[dbo].[{objectName}]'), 0) AS ObjectID";
        }

        public static void CreateScriptFile(string sqlFilePath, string sqlStatement)
        {
            using (var sw = File.CreateText(sqlFilePath))
            {
                sw.WriteLine(sqlStatement);
            }
        }

        public static string CreateCsvTableScript(string tableName)
        {
            return $@"
                IF (NOT EXISTS(SELECT 1 FROM [sys].[objects] WHERE type = 'U' AND name = '{tableName}'))
                BEGIN
                    CREATE TABLE [dbo].[{tableName}](
	                    [FirstName] [nvarchar](50) NULL,
	                    [LastName] [nvarchar](50) NULL,
	                    [BirthDate] [datetime] NULL
                    );
                END
            ";
        }
        public static string CreateCsvTableScript2()
        {
            var sqlStatement = @"IF (NOT EXISTS(SELECT 1 FROM [sys].[objects] WHERE type = 'U' AND name = 'CompleteTable'))
                BEGIN
	                CREATE TABLE [dbo].[CompleteTable]
	                (
		                [ColumnBigInt] BIGINT NULL,
		                [ColumnBinary] BINARY(4000) NULL,
		                [ColumnBit] BIT NULL,
		                [ColumnChar] CHAR(32) NULL,
		                [ColumnDate] DATE NULL,
		                [ColumnDateTime] DATETIME NULL,
		                [ColumnDateTime2] DATETIME2(7) NULL,
		                [ColumnDateTimeOffset] DATETIMEOFFSET(7) NULL,
		                [ColumnDecimal] DECIMAL(18, 2) NULL,
		                [ColumnFloat] FLOAT NULL,
		                [ColumnGeography] GEOGRAPHY NULL,
		                [ColumnGeometry] GEOMETRY NULL,
		                [ColumnHierarchyId] HIERARCHYID NULL,
		                [ColumnImage] IMAGE NULL,
		                [ColumnInt] INT NULL,
		                [ColumnMoney] MONEY NULL,
		                [ColumnNChar] NCHAR(32) NULL,
		                [ColumnNText] NTEXT NULL,
		                [ColumnNumeric] NUMERIC(18, 2) NULL,
		                [ColumnNVarChar] NVARCHAR(MAX) NULL,
		                [ColumnReal] REAL NULL,
		                [ColumnSmallDateTime] SMALLDATETIME NULL,
		                [ColumnSmallInt] SMALLINT NULL,
		                [ColumnSmallMoney] SMALLMONEY NULL,
		                [ColumnSqlVariant] SQL_VARIANT NULL,
		                [ColumnText] TEXT NULL,
		                [ColumnTime] TIME(7) NULL,
		                [ColumnTimestamp] TIMESTAMP NULL,
		                [ColumnTinyInt] TINYINT NULL,
		                [ColumnUniqueIdentifier] UNIQUEIDENTIFIER NULL,
		                [ColumnVarBinary] VARBINARY(MAX) NULL,
		                [ColumnVarChar] VARCHAR(MAX) NULL,
		                [ColumnXml] XML NULL,
	                );
                END";

            return sqlStatement;
        }

    }
}
