using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ArdiLabs.Yuniql
{
    public class MigrationService
    {
        public void Run(string workingPath, string connectionString, bool autoCreateDatabase = false, string targetVersion = null)
        {
            var targetSqlDbConnectionString = new SqlConnectionStringBuilder(connectionString);

            //check if database exists and auto-create if required
            var masterSqlDbConnectionString = new SqlConnectionStringBuilder(connectionString);
            masterSqlDbConnectionString.InitialCatalog = "master";

            var targetDatabaseExists = IsTargetDatabaseExists(masterSqlDbConnectionString, targetSqlDbConnectionString.InitialCatalog);
            if (!targetDatabaseExists && autoCreateDatabase)
            {
                CreateDatabase(masterSqlDbConnectionString, targetSqlDbConnectionString.InitialCatalog);
            }

            //check database has been pre-configured to support migration
            var targetDatabaseConfigured = IsTargetDatabaseConfigured(targetSqlDbConnectionString);
            if (!targetDatabaseConfigured)
            {
                ConfigureDatabase(targetSqlDbConnectionString);
            }

            //runs all scripts in the _pre folder

            //runs all scripts int the version folders
            var currentVersion = GetCurrentVersion(targetSqlDbConnectionString);
            var dbVersions = GetAllDbVersions(targetSqlDbConnectionString)
                    .Select(dv => dv.Version)
                    .OrderBy(v => v);

            //excludes all versions already executed
            var versionFolders = Directory.GetDirectories(workingPath, "v*.*")
                .Where(v => !dbVersions.Contains(new DirectoryInfo(v).Name))
                .ToList();

            //exclude all versions greater than the target version
            if (!string.IsNullOrEmpty(targetVersion))
            {
                versionFolders.RemoveAll(v =>
                {
                    var cv = new LocalVersion(new DirectoryInfo(v).Name);
                    var tv = new LocalVersion(targetVersion);

                    return string.Compare(cv.SemVersion, tv.SemVersion) == 1;
                });
            }

            if (versionFolders.Any())
            {

                versionFolders.Sort();
                versionFolders.ForEach(versionFolder =>
                {
                    RunMigrationStep(targetSqlDbConnectionString, versionFolder);
                });
            }
            else
            {
                Console.WriteLine("DB runs the latest version already.");
            }

        }

        public bool IsTargetDatabaseExists(SqlConnectionStringBuilder sqlConnectionString, string targetDatabaseName)
        {
            var sqlStatement = $"SELECT ISNULL(DB_ID (N'{targetDatabaseName}'),0);";
            var result = DbHelper.QuerySingleBool(sqlConnectionString, sqlStatement);

            return result;
        }

        public void CreateDatabase(SqlConnectionStringBuilder sqlConnectionString, string targetDatabaseName)
        {
            var sqlStatement = $"CREATE DATABASE {targetDatabaseName};";
            DbHelper.ExecuteNonQuery(sqlConnectionString, sqlStatement);
        }

        public bool IsTargetDatabaseConfigured(SqlConnectionStringBuilder sqlConnectionString)
        {
            var sqlStatement = $"SELECT ISNULL(OBJECT_ID('dbo.__DbVersion'),0) AS ObjId";
            var result = DbHelper.QuerySingleBool(sqlConnectionString, sqlStatement);

            return result;
        }

        public void ConfigureDatabase(SqlConnectionStringBuilder sqlConnectionString)
        {
            var sqlStatement = $@"
                    USE {sqlConnectionString.InitialCatalog};
                    IF OBJECT_ID('[dbo].[__DbVersion]') IS NULL 
                    BEGIN
                        CREATE TABLE[dbo].[__DbVersion](        
                            [Id][int] IDENTITY(1, 1) NOT NULL,        
                            [Version] [NVARCHAR] (10) NOT NULL,
                            [Created] [DATETIME2] NOT NULL,         
                            [CreatedBy] [NVARCHAR] (200) NULL,
                        CONSTRAINT[PK___DbVersion] PRIMARY KEY CLUSTERED
                        (
                            [Id] ASC
                        ) WITH(
                            PAD_INDEX = OFF, 
                            STATISTICS_NORECOMPUTE = OFF, 
                            IGNORE_DUP_KEY = OFF, 
                            ALLOW_ROW_LOCKS = ON, 
                            ALLOW_PAGE_LOCKS = ON
                        ) ON [PRIMARY]
                        ) ON [PRIMARY];

                        ALTER TABLE [dbo].[__DbVersion] ADD CONSTRAINT[DF___DbVersion_Created]  DEFAULT(GETUTCDATE()) FOR [Created];
                        ALTER TABLE [dbo].[__DbVersion] ADD CONSTRAINT[DF___DbVersion_CreatedBy]  DEFAULT(SUSER_SNAME()) FOR [CreatedBy];

                        INSERT INTO [dbo].[__DbVersion] (Version) VALUES('v0.00');
                    END
                ";

            using (var connection = new SqlConnection(sqlConnectionString.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
            }
        }

        public string GetCurrentVersion(SqlConnectionStringBuilder sqlConnectionString)
        {
            var sqlStatement = $"SELECT TOP 1 Version FROM dbo.__DbVersion ORDER BY Id DESC";
            var result = DbHelper.QuerySingleString(sqlConnectionString, sqlStatement);

            return result;
        }

        public void IncrementVersion(SqlConnectionStringBuilder sqlConnectionString, string nextVersion)
        {
            var sqlStatement = $"INSERT INTO dbo.__DbVersion (Version) VALUES (N'{nextVersion}')";
            DbHelper.ExecuteScalar(sqlConnectionString, sqlStatement);
        }

        public List<DbVersion> GetAllDbVersions(SqlConnectionStringBuilder sqlConnectionString)
        {
            var result = new List<DbVersion>();

            var sqlStatement = $"SELECT Id, Version FROM dbo.__DbVersion ORDER BY Version ASC;";
            using (var connection = new SqlConnection(sqlConnectionString.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var dbVersion = new DbVersion
                    {
                        Id = reader.GetInt32(0),
                        Version = reader.GetString(1)
                    };
                    result.Add(dbVersion);
                }
            }
            return result;
        }

        public void RunMigrationStep(SqlConnectionStringBuilder sqlConnectionString, string versionFolder)
        {
            var sqlScriptFiles = Directory.GetFiles(versionFolder, "*.sql").ToList();

            using (var connection = new SqlConnection(sqlConnectionString.ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        //execute all script files in the version folder
                        sqlScriptFiles.ForEach(scriptFile =>
                        {
                            //https://stackoverflow.com/questions/25563876/executing-sql-batch-containing-go-statements-in-c-sharp/25564722#25564722
                            var sqlStatementFile = File.ReadAllText(scriptFile);
                            var sqlStatements = Regex.Split(sqlStatementFile, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .ToList()
;
                            sqlStatements.ForEach(sqlStatement =>
                            {
                                var command = connection.CreateCommand();
                                command.Transaction = transaction;
                                command.CommandType = CommandType.Text;
                                command.CommandText = sqlStatement;
                                command.CommandTimeout = 0;
                                command.ExecuteNonQuery();
                            });
                        });

                        //increment db version
                        var versionName = new DirectoryInfo(versionFolder).Name;
                        var incrementVersionSqlStatement = $"INSERT INTO dbo.__DbVersion (Version) VALUES ('{versionName.Substring(versionName.IndexOf("-") + 1)}')";
                        var incrementVersioncommand = connection.CreateCommand();
                        incrementVersioncommand.Transaction = transaction;
                        incrementVersioncommand.CommandType = CommandType.Text;
                        incrementVersioncommand.CommandText = incrementVersionSqlStatement;
                        incrementVersioncommand.CommandTimeout = 0;
                        incrementVersioncommand.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
