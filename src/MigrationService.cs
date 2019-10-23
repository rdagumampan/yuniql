using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArdiLabs.Yuniql
{
    public class MigrationService : IMigrationService
    {
        private readonly string connectionString;

        public MigrationService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void Run(string workingPath,
            string targetVersion,
            bool autoCreateDatabase,
            List<KeyValuePair<string, string>> tokens = null)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var targetDatabaseName = connectionStringBuilder.InitialCatalog;
            var targetDatabaseServer = connectionStringBuilder.DataSource;

            //create the database, we need this to be outside of the transaction scope
            //in an event of failure, users have to manually drop the auto-created database
            var targetDatabaseExists = IsTargetDatabaseExists(targetDatabaseName);
            if (!targetDatabaseExists && autoCreateDatabase)
            {
                CreateDatabase(targetDatabaseName);
                TraceService.Info($"Created database {targetDatabaseName} on {targetDatabaseServer}.");
            }

            //check if database has been pre-configured to support migration and setup when its not
            var targetDatabaseConfigured = IsTargetDatabaseConfigured();
            if (!targetDatabaseConfigured)
            {
                ConfigureDatabase(targetDatabaseName);
                TraceService.Info($"Configured database migration support for {targetDatabaseName} on {targetDatabaseServer}.");
            }

            //enclose all executions in a single transaction
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        //check if database has been pre-configured and execute init scripts
                        if (!targetDatabaseConfigured)
                        {
                            //runs all scripts in the _init folder
                            RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_init"), tokens);
                            TraceService.Info($"Executed script files on {Path.Combine(workingPath, "_init")}");
                        }

                        //checks if target database already runs the latest version and skips work if it already is
                        if (!IsTargetDatabaseLatest(connection, transaction, targetVersion))
                        {
                            //runs all scripts in the _pre folder and subfolders
                            RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_pre"), tokens);
                            TraceService.Info($"Executed script files on {Path.Combine(workingPath, "_pre")}");

                            //runs all scripts int the vxx.xx folders and subfolders
                            RunVersionScripts(connection, transaction, workingPath, targetVersion, tokens);

                            //runs all scripts in the _draft folder and subfolders
                            RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_draft"), tokens);
                            TraceService.Info($"Executed script files on {Path.Combine(workingPath, "_draft")}");

                            //runs all scripts in the _post folder and subfolders
                            RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_post"), tokens);
                            TraceService.Info($"Executed script files on {Path.Combine(workingPath, "_post")}");
                        }
                        else
                        {
                            TraceService.Info($"Target database is updated. No changes made at {targetDatabaseName} on {targetDatabaseServer}.");
                        }

                        //commit all changes
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
        private bool IsTargetDatabaseExists(string targetDatabaseName)
        {
            var sqlStatement = $"SELECT ISNULL(DB_ID (N'{targetDatabaseName}'),0);";

            //check if database exists and auto-create when its not
            var masterConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            masterConnectionStringBuilder.InitialCatalog = "master";

            var result = DbHelper.QuerySingleBool(masterConnectionStringBuilder.ConnectionString, sqlStatement);

            return result;
        }

        private bool IsTargetDatabaseConfigured()
        {
            var sqlStatement = $"SELECT ISNULL(OBJECT_ID('dbo.__YuniqlDbVersion'),0) IsDatabaseConfigured";
            var result = DbHelper.QuerySingleBool(connectionString, sqlStatement);

            return result;
        }

        private bool IsTargetDatabaseLatest(string targetVersion)
        {
            var dbcv = GetCurrentVersion();
            if (string.IsNullOrEmpty(dbcv)) return false;

            var cv = new LocalVersion(dbcv);
            var tv = new LocalVersion(targetVersion);
            return string.Compare(cv.SemVersion, tv.SemVersion) == 1 || //db has more updated than local version
                string.Compare(cv.SemVersion, tv.SemVersion) == 0;      //db has the same version as local version
        }

        private void CreateDatabase(string targetDatabaseName)
        {
            var sqlStatement = $"CREATE DATABASE {targetDatabaseName};";
            var masterConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            masterConnectionStringBuilder.InitialCatalog = "master";

            DbHelper.ExecuteNonQuery(masterConnectionStringBuilder.ConnectionString, sqlStatement);
        }

        private void ConfigureDatabase(string targetDatabaseName)
        {
            var sqlStatement = $@"
                    USE {targetDatabaseName};

                    IF OBJECT_ID('[dbo].[__YuniqlDbVersion]') IS NULL 
                    BEGIN
	                    CREATE TABLE [dbo].[__YuniqlDbVersion](
		                    [Id] [SMALLINT] IDENTITY(1,1),
		                    [Version] [NVARCHAR](32) NOT NULL,
		                    [DateInsertedUtc] [DATETIME] NOT NULL,
		                    [LastUpdatedUtc] [DATETIME] NOT NULL,
		                    [LastUserId] [NVARCHAR](128) NOT NULL,
		                    [Artifact] [VARBINARY](MAX) NULL,
		                    CONSTRAINT [PK___YuniqlDbVersion] PRIMARY KEY CLUSTERED ([Id] ASC),
		                    CONSTRAINT [IX___YuniqlDbVersion] UNIQUE NONCLUSTERED ([Version] ASC)
	                    );

	                    ALTER TABLE [dbo].[__YuniqlDbVersion] ADD  CONSTRAINT [DF___YuniqlDbVersion_DateInsertedUtc]  DEFAULT (GETUTCDATE()) FOR [DateInsertedUtc];
	                    ALTER TABLE [dbo].[__YuniqlDbVersion] ADD  CONSTRAINT [DF___YuniqlDbVersion_LastUpdatedUtc]  DEFAULT (GETUTCDATE()) FOR [LastUpdatedUtc];
	                    ALTER TABLE [dbo].[__YuniqlDbVersion] ADD  CONSTRAINT [DF___YuniqlDbVersion_LastUserId]  DEFAULT (SUSER_SNAME()) FOR [LastUserId];
                    END                
            ";

            TraceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
            }
        }

        private string GetCurrentVersion()
        {
            var sqlStatement = $"SELECT TOP 1 Version FROM [dbo].[__YuniqlDbVersion] ORDER BY Id DESC;";
            var result = DbHelper.QuerySingleString(connectionString, sqlStatement);

            return result;
        }

        public List<DbVersion> GetAllDbVersions()
        {
            var result = new List<DbVersion>();

            var sqlStatement = $"SELECT Id, Version, DateInsertedUtc, LastUserId FROM [dbo].[__YuniqlDbVersion] ORDER BY Version ASC;";
            TraceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            using (var connection = new SqlConnection(connectionString))
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
                        Id = reader.GetInt16(0),
                        Version = reader.GetString(1),
                        DateInsertedUtc = reader.GetDateTime(2),
                        LastUserId = reader.GetString(3)
                    };
                    result.Add(dbVersion);
                }
            }

            return result;
        }

        private void RunNonVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            string directoryFullPath,
            List<KeyValuePair<string, string>> tokens = null)
        {
            var sqlScriptFiles = Directory.GetFiles(directoryFullPath, "*.sql").ToList();
            TraceService.Info($"Found the {sqlScriptFiles.Count} script files on {directoryFullPath}");
            TraceService.Info($"{string.Join(@"\r\n\t", sqlScriptFiles.Select(s => new FileInfo(s).Name))}");

            //execute all script files in the target folder
            sqlScriptFiles.ForEach(scriptFile =>
            {
                //https://stackoverflow.com/questions/25563876/executing-sql-batch-containing-go-statements-in-c-sharp/25564722#25564722
                var sqlStatementRaw = File.ReadAllText(scriptFile);
                var sqlStatements = Regex.Split(sqlStatementRaw, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList()
    ;
                sqlStatements.ForEach(sqlStatement =>
                {
                    //replace tokens with values from the cli
                    var tokeReplacementService = new TokenReplacementService();
                    sqlStatement = tokeReplacementService.Replace(tokens, sqlStatement);

                    TraceService.Debug($"Executing sql statement as part of : {scriptFile}{Environment.NewLine}{sqlStatement}");

                    var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sqlStatement;
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();
                });

                TraceService.Info($"Executed script file {scriptFile}.");
            });
        }

        private void RunVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            string workingPath,
            string targetVersion,
            List<KeyValuePair<string, string>> tokens = null)
        {
            var dbVersions = GetAllDbVersions()
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

            //execute all sql scripts in the version folders
            if (versionFolders.Any())
            {
                versionFolders.Sort();
                versionFolders.ForEach(versionDirectory =>
                {
                    try
                    {
                        //run scripts in all sub-directories
                        var versionSubDirectories = Directory.GetDirectories(versionDirectory, "*", SearchOption.AllDirectories).ToList();
                        versionSubDirectories.ForEach(versionSubDirectory =>
                        {
                            RunMigrationScriptsInternal(connection, transaction, versionSubDirectory, tokens);
                        });

                        //run all scripts in the current version folder
                        RunMigrationScriptsInternal(connection, transaction, versionDirectory, tokens);

                        //import csv files into tables of the the same filename as the csv
                        RunCsvImport(connection, transaction, versionDirectory);

                        //update db version
                        var versionName = new DirectoryInfo(versionDirectory).Name;
                        UpdateDbVersion(connection, transaction, versionName);

                        TraceService.Info($"Completed migration to version {versionDirectory}");
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                });
            }
            else
            {
                var connectionString = new SqlConnectionStringBuilder(connection.ConnectionString);
                TraceService.Info($"Target database is updated. No migration step executed at {connection.Database} on {connectionString.DataSource}.");
            }
        }

        private void RunCsvImport(
            IDbConnection connection,
            IDbTransaction transaction,
            string versionFullPath)
        {
            //execute all script files in the version folder
            var csvFiles = Directory.GetFiles(versionFullPath, "*.csv").ToList();
            TraceService.Info($"Found the {csvFiles.Count} csv files on {versionFullPath}");
            TraceService.Info($"{string.Join(@"\r\n\t", csvFiles.Select(s => new FileInfo(s).Name))}");

            connection.Open();
            csvFiles.ForEach(csvFile =>
            {
                var csvImportService = new CsvImportService();
                csvImportService.Run(connection, transaction, csvFile);
                TraceService.Info($"Imported csv file {csvFile}.");
            });
        }

        private void RunMigrationScriptsInternal(
            IDbConnection connection,
            IDbTransaction transaction,
            string versionFullPath,
            List<KeyValuePair<string, string>> tokens = null)
        {
            var sqlScriptFiles = Directory.GetFiles(versionFullPath, "*.sql").ToList();
            TraceService.Info($"Found the {sqlScriptFiles.Count} script files on {versionFullPath}");
            TraceService.Info($"{string.Join(@"\r\n\t", sqlScriptFiles.Select(s => new FileInfo(s).Name))}");

            //execute all script files in the version folder
            sqlScriptFiles.ForEach(scriptFile =>
            {
                //https://stackoverflow.com/questions/25563876/executing-sql-batch-containing-go-statements-in-c-sharp/25564722#25564722
                var sqlStatementRaw = File.ReadAllText(scriptFile);
                var sqlStatements = Regex.Split(sqlStatementRaw, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList()
    ;
                sqlStatements.ForEach(sqlStatement =>
                {
                    //replace tokens with values from the cli
                    var tokeReplacementService = new TokenReplacementService();
                    sqlStatement = tokeReplacementService.Replace(tokens, sqlStatement);

                    TraceService.Debug($"Executing sql statement as part of : {scriptFile}{Environment.NewLine}{sqlStatement}");

                    var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sqlStatement;
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();
                });

                TraceService.Info($"Executed script file {scriptFile}.");
            });
        }

        private void UpdateDbVersion(
            IDbConnection connection,
            IDbTransaction transaction,
            string version)
        {
            var incrementVersionSqlStatement = $"INSERT INTO [dbo].[__YuniqlDbVersion] (Version) VALUES ('{version}');";
            TraceService.Debug($"Executing sql statement: {Environment.NewLine}{incrementVersionSqlStatement}");

            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = incrementVersionSqlStatement;
            command.CommandTimeout = 0;
            command.ExecuteNonQuery();
        }
    }
}
