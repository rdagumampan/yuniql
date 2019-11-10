using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ArdiLabs.Yuniql
{
    public class MigrationService : IMigrationService
    {
        private readonly IDataService _dataService;
        private readonly ICsvImportService _csvImportService;

        public MigrationService(IDataService dataService, ICsvImportService csvImportService)
        {
            this._dataService = dataService;
            this._csvImportService = csvImportService;
        }

        public void Initialize(string connectionString)
        {
            //initialize dependencies
            _dataService.Initialize(connectionString);
            _csvImportService.Initialize(connectionString);
        }

        public void Run(
            string workingPath,
            string targetVersion,
            bool autoCreateDatabase,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            bool verifyOnly = false)
        {
            var connectionInfo = _dataService.GetConnectionInfo();
            var targetDatabaseName = connectionInfo.Database;
            var targetDatabaseServer = connectionInfo.DataSource;

            //create the database, we need this to be outside of the transaction scope
            //in an event of failure, users have to manually drop the auto-created database
            var targetDatabaseExists = _dataService.IsTargetDatabaseExists();
            if (!targetDatabaseExists && autoCreateDatabase)
            {
                TraceService.Info($"Target database does not exist. Creating database {targetDatabaseName} on {targetDatabaseServer}.");
                _dataService.CreateDatabase();
                TraceService.Info($"Created database {targetDatabaseName} on {targetDatabaseServer}.");
            }

            //check if database has been pre-configured to support migration and setup when its not
            var targetDatabaseConfigured = _dataService.IsTargetDatabaseConfigured();
            if (!targetDatabaseConfigured)
            {
                TraceService.Info($"Target database {targetDatabaseName} on {targetDatabaseServer} not yet configured for migration.");
                _dataService.ConfigureDatabase();
                TraceService.Info($"Configured database migration support for {targetDatabaseName} on {targetDatabaseServer}.");
            }

            var dbVersions = _dataService.GetAllVersions()
                .Select(dv => dv.Version)
                .OrderBy(v => v)
                .ToList();

            var targeDatabaseLatest = IsTargetDatabaseLatest(targetVersion);
            if (!targeDatabaseLatest)
            {
                //enclose all executions in a single transaction
                using (var connection = _dataService.CreateConnection())
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
                                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_init"), tokenKeyPairs);
                                TraceService.Info($"Executed script files on {Path.Combine(workingPath, "_init")}");
                            }

                            //checks if target database already runs the latest version and skips work if it already is
                            //runs all scripts in the _pre folder and subfolders
                            RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_pre"), tokenKeyPairs);
                            TraceService.Info($"Executed script files on {Path.Combine(workingPath, "_pre")}");

                            //runs all scripts int the vxx.xx folders and subfolders
                            RunVersionScripts(connection, transaction, dbVersions, workingPath, targetVersion, tokenKeyPairs);

                            //runs all scripts in the _draft folder and subfolders
                            RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_draft"), tokenKeyPairs);
                            TraceService.Info($"Executed script files on {Path.Combine(workingPath, "_draft")}");

                            //runs all scripts in the _post folder and subfolders
                            RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_post"), tokenKeyPairs);
                            TraceService.Info($"Executed script files on {Path.Combine(workingPath, "_post")}");

                            //when true, the execution is an uncommitted transaction 
                            //and only for purpose of testing if all can go well when it run to the target environment
                            if (verifyOnly)
                                transaction.Rollback();
                            else
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
            else
            {
                TraceService.Info($"Target database runs the latest version already. No changes made at {targetDatabaseName} on {targetDatabaseServer}.");
            }
        }

        private bool IsTargetDatabaseLatest(string targetVersion)
        {
            var dbcv = _dataService.GetCurrentVersion();
            if (string.IsNullOrEmpty(dbcv)) return false;

            var cv = new LocalVersion(dbcv);
            var tv = new LocalVersion(targetVersion);
            return string.Compare(cv.SemVersion, tv.SemVersion) == 1 || //db has more updated than local version
                string.Compare(cv.SemVersion, tv.SemVersion) == 0;      //db has the same version as local version
        }
        
        public string GetCurrentVersion()
        {
            return _dataService.GetCurrentVersion();
        }

        public List<DbVersion> GetAllVersions()
        {
            return _dataService.GetAllVersions();
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
                var sqlStatements = _dataService.BreakStatements(sqlStatementRaw);

                sqlStatements.ForEach(sqlStatement =>
                {
                    //replace tokens with values from the cli
                    var tokeReplacementService = new TokenReplacementService();
                    sqlStatement = tokeReplacementService.Replace(tokens, sqlStatement);

                    TraceService.Debug($"Executing sql statement as part of : {scriptFile}{Environment.NewLine}{sqlStatement}");
                    _dataService.ExecuteNonQuery(connection, sqlStatement, transaction);
                });

                TraceService.Info($"Executed script file {scriptFile}.");
            });
        }

        private void RunVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            List<string> dbVersions,
            string workingPath,
            string targetVersion,
            List<KeyValuePair<string, string>> tokens = null)
        {
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
                        versionSubDirectories.Sort();
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
                        _dataService.UpdateVersion(connection, transaction, versionName);

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
                var connectionInfo = _dataService.GetConnectionInfo();
                TraceService.Info($"Target database is updated. No migration step executed at {connectionInfo.Database} on {connectionInfo.DataSource}.");
            }
        }

        private void RunCsvImport(
            IDbConnection connection,
            IDbTransaction transaction,
            string versionFullPath)
        {
            //execute all script files in the version folder
            var csvFiles = Directory.GetFiles(versionFullPath, "*.csv").ToList();
            csvFiles.Sort();

            TraceService.Info($"Found the {csvFiles.Count} csv files on {versionFullPath}");
            TraceService.Info($"{string.Join(@"\r\n\t", csvFiles.Select(s => new FileInfo(s).Name))}");

            csvFiles.ForEach(csvFile =>
            {
                _csvImportService.Run(connection, transaction, csvFile);
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
            sqlScriptFiles.Sort();
            sqlScriptFiles
                .ForEach(scriptFile =>
            {
                //https://stackoverflow.com/questions/25563876/executing-sql-batch-containing-go-statements-in-c-sharp/25564722#25564722
                var sqlStatementRaw = File.ReadAllText(scriptFile);
                var sqlStatements = _dataService.BreakStatements(sqlStatementRaw);
    ;
                sqlStatements.ForEach(sqlStatement =>
                {
                    //replace tokens with values from the cli
                    var tokeReplacementService = new TokenReplacementService();
                    sqlStatement = tokeReplacementService.Replace(tokens, sqlStatement);

                    TraceService.Debug($"Executing sql statement as part of : {scriptFile}{Environment.NewLine}{sqlStatement}");
                    _dataService.ExecuteNonQuery(connection, sqlStatement, transaction);
                });

                TraceService.Info($"Executed script file {scriptFile}.");
            });
        }

        public void Erase(string workingPath)
        {
            //enclose all executions in a single transaction
            using (var connection = _dataService.CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        //runs all scripts in the _erase folder
                        RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_erase"));
                        TraceService.Info($"Executed script files on {Path.Combine(workingPath, "_erase")}");

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
