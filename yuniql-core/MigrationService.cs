using Yuniql.Extensibility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Yuniql.Core
{
    /// <inheritdoc />
    public class MigrationService : MigrationServiceBase
    {
        private readonly ILocalVersionService _localVersionService;
        private readonly IDataService _dataService;
        private readonly IBulkImportService _bulkImportService;
        private readonly ITokenReplacementService _tokenReplacementService;
        private readonly IDirectoryService _directoryService;
        private readonly IFileService _fileService;
        private readonly ITraceService _traceService;
        private readonly IConfigurationDataService _configurationDataService;

        /// <inheritdoc />
        public MigrationService(
            ILocalVersionService localVersionService,
            IDataService dataService,
            IBulkImportService bulkImportService,
            IConfigurationDataService configurationDataService,
            ITokenReplacementService tokenReplacementService,
            IDirectoryService directoryService,
            IFileService fileService,
            ITraceService traceService)
            : base(
                localVersionService,
                dataService,
                bulkImportService,
                configurationDataService,
                tokenReplacementService,
                directoryService,
                fileService,
                traceService
            )
        {
            this._localVersionService = localVersionService;
            this._dataService = dataService;
            this._bulkImportService = bulkImportService;
            this._tokenReplacementService = tokenReplacementService;
            this._directoryService = directoryService;
            this._fileService = fileService;
            this._traceService = traceService;
            this._configurationDataService = configurationDataService;
        }

        /// <inheritdoc />
        public override void Run(
            string workingPath,
            string targetVersion = null,
            bool? autoCreateDatabase = false,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            bool? verifyOnly = false,
            string bulkSeparator = null,
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null,
            int? bulkBatchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environmentCode = null,
            NonTransactionalResolvingOption? resumeFromFailure = null
         )
        {
            //check the workspace structure if required directories are present
            _localVersionService.Validate(workingPath);

            //when uncomitted run is not supported, fail migration, throw exceptions and return error exit code
            if (verifyOnly.HasValue && verifyOnly == true && !_dataService.IsAtomicDDLSupported)
            {
                throw new NotSupportedException("Yuniql.Verify is not supported in the target platform. " +
                    "The feature requires support for atomic DDL operations. " +
                    "An atomic DDL operations ensures creation of tables, views and other objects and data are rolledback in case of error. " +
                    "For more information see https://yuniql.io/docs/.");
            }

            //when no target version specified, we use the latest local version available
            if (string.IsNullOrEmpty(targetVersion))
            {
                targetVersion = _localVersionService.GetLatestVersion(workingPath);
                _traceService.Info($"No explicit target version requested. We'll use latest available locally {targetVersion} on {workingPath}.");
            }

            var connectionInfo = _dataService.GetConnectionInfo();
            var targetDatabaseName = connectionInfo.Database;
            var targetDatabaseServer = connectionInfo.DataSource;

            //we try to auto-create the database, we need this to be outside of the transaction scope
            //in an event of failure, users have to manually drop the auto-created database!
            //we only check if the db exists when --auto-create-db is true
            if (autoCreateDatabase.HasValue && autoCreateDatabase == true)
            {
                //we only check if the db exists when --auto-create-db is true
                var targetDatabaseExists = _configurationDataService.IsDatabaseExists();
                if (!targetDatabaseExists)
                {
                    _traceService.Info($"Target database does not exist. Creating database {targetDatabaseName} on {targetDatabaseServer}.");
                    _configurationDataService.CreateDatabase();
                    _traceService.Info($"Created database {targetDatabaseName} on {targetDatabaseServer}.");
                }
            }

            //check if database has been pre-configured to support migration and setup when its not
            var targetDatabaseConfigured = _configurationDataService.IsDatabaseConfigured(metaSchemaName, metaTableName);
            if (!targetDatabaseConfigured)
            {
                //create custom schema when user supplied and only if platform supports it
                if (_dataService.IsSchemaSupported && null != metaSchemaName && !_dataService.SchemaName.Equals(metaSchemaName))
                {
                    _traceService.Info($"Target schema does not exist. Creating schema {metaSchemaName} on {targetDatabaseName} on {targetDatabaseServer}.");
                    _configurationDataService.CreateSchema(metaSchemaName);
                    _traceService.Info($"Created schema {metaSchemaName} on {targetDatabaseName} on {targetDatabaseServer}.");
                }

                //create empty versions tracking table
                _traceService.Info($"Target database {targetDatabaseName} on {targetDatabaseServer} not yet configured for migration.");
                _configurationDataService.ConfigureDatabase(metaSchemaName, metaTableName);
                _traceService.Info($"Configured database migration support for {targetDatabaseName} on {targetDatabaseServer}.");
            }

            var allVersions = _configurationDataService.GetAllVersions(metaSchemaName, metaTableName)
                .Select(dv => dv.Version)
                .OrderBy(v => v)
                .ToList();

            //check if target database already runs the latest version and skips work if it already is
            var targeDatabaseLatest = IsTargetDatabaseLatest(targetVersion, metaSchemaName, metaTableName);
            if (!targeDatabaseLatest)
            {
                //enclose all executions in a single transaction, in the event of failure we roll back everything
                using (var connection = _dataService.CreateConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            //run all migrations present in all directories
                            RunAllInternal(connection, transaction);

                            //when true, the execution is an uncommitted transaction 
                            //and only for purpose of testing if all can go well when it run to the target environment
                            if (verifyOnly.HasValue && verifyOnly == true)
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
                //enclose all executions in a single transaction
                using (var connection = _dataService.CreateConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            //run all scripts present in the _pre, _draft and _post directories
                            RunDraftInternal(connection, transaction);

                            //when true, the execution is an uncommitted transaction 
                            //and only for purpose of testing if all can go well when it run to the target environment
                            if (verifyOnly.HasValue && verifyOnly == true)
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
                _traceService.Info($"Target database runs the latest version already. Scripts in _pre, _draft and _post are executed.");
            }

            //local method
            void RunAllInternal(IDbConnection connection, IDbTransaction transaction)
            {
                //check if database has been pre-configured and execute init scripts
                if (!targetDatabaseConfigured)
                {
                    //runs all scripts in the _init folder
                    RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_init"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                    _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_init")}");
                }

                //checks if target database already runs the latest version and skips work if it already is
                //runs all scripts in the _pre folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_pre"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_pre")}");

                //runs all scripts int the vxx.xx folders and subfolders
                RunVersionScripts(connection, transaction, allVersions, workingPath, targetVersion, null, tokenKeyPairs, bulkSeparator: bulkSeparator, metaSchemaName: metaSchemaName, metaTableName: metaTableName, commandTimeout: commandTimeout, bulkBatchSize: bulkBatchSize, appliedByTool: appliedByTool, appliedByToolVersion: appliedByToolVersion, environmentCode: environmentCode);

                //runs all scripts in the _draft folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_draft"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_draft")}");

                //runs all scripts in the _post folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_post"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_post")}");
            }

            //local method
            void RunDraftInternal(IDbConnection connection, IDbTransaction transaction)
            {
                //runs all scripts in the _pre folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_pre"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_pre")}");

                //runs all scripts in the _draft folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_draft"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_draft")}");

                //runs all scripts in the _post folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_post"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_post")}");
            }
        }

        ///<inheritdoc/>
        public override void RunVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            List<string> dbVersions,
            string workingPath,
            string targetVersion,
            NonTransactionalContext nonTransactionalContext,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            string bulkSeparator = null,
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null,
            int? bulkBatchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environmentCode = null
        )
        {
            //excludes all versions already executed
            var versionDirectories = _directoryService.GetDirectories(workingPath, "v*.*")
                .Where(v => !dbVersions.Contains(new DirectoryInfo(v).Name))
                .ToList();

            //exclude all versions greater than the target version
            if (!string.IsNullOrEmpty(targetVersion))
            {
                versionDirectories.RemoveAll(v =>
                {
                    var cv = new LocalVersion(new DirectoryInfo(v).Name);
                    var tv = new LocalVersion(targetVersion);

                    return string.Compare(cv.SemVersion, tv.SemVersion) == 1;
                });
            }

            //execute all sql scripts in the version folders
            if (versionDirectories.Any())
            {
                versionDirectories.Sort();
                versionDirectories.ForEach(versionDirectory =>
                {
                    var versionName = new DirectoryInfo(versionDirectory).Name;

                    //run scripts in all sub-directories
                    var scriptSubDirectories = _directoryService.GetAllDirectories(versionDirectory, "*").ToList();
                    scriptSubDirectories.Sort();
                    scriptSubDirectories.ForEach(scriptSubDirectory =>
                    {
                        //run all scripts in the current version folder
                        RunSqlScripts(connection, transaction, nonTransactionalContext, versionName, workingPath, scriptSubDirectory, metaSchemaName, metaTableName, tokenKeyPairs, commandTimeout, environmentCode);

                        //import csv files into tables of the the same filename as the csv
                        RunBulkImport(connection, transaction, workingPath, scriptSubDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environmentCode);
                    });

                    //run all scripts in the current version folder
                    RunSqlScripts(connection, transaction, nonTransactionalContext, versionName, workingPath, versionDirectory, metaSchemaName, metaTableName, tokenKeyPairs, commandTimeout, environmentCode);

                    //import csv files into tables of the the same filename as the csv
                    RunBulkImport(connection, transaction, workingPath, versionDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environmentCode);

                    //update db version
                    _configurationDataService.InsertVersion(connection, transaction, versionName,
                        metaSchemaName: metaSchemaName,
                        metaTableName: metaTableName,
                        commandTimeout: commandTimeout,
                        appliedByTool: appliedByTool,
                        appliedByToolVersion: appliedByToolVersion);

                    _traceService.Info($"Completed migration to version {versionDirectory}");
                });
            }
            else
            {
                var connectionInfo = _dataService.GetConnectionInfo();
                _traceService.Info($"Target database is updated. No migration step executed at {connectionInfo.Database} on {connectionInfo.DataSource}.");
            }
        }

        ///<inheritdoc/>
        public override void RunSqlScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            NonTransactionalContext nonTransactionalContext,
            string version,
            string workingPath,
            string scriptDirectory,
            string metaSchemaName,
            string metaTableName,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            int? commandTimeout = null,
            string environmentCode = null,
            string appliedByTool = null,
            string appliedByToolVersion = null
        )
        {
            //extract and filter out scripts when environment code is used
            var sqlScriptFiles = _directoryService.GetFiles(scriptDirectory, "*.sql").ToList();
            sqlScriptFiles = _directoryService.FilterFiles(workingPath, environmentCode, sqlScriptFiles).ToList();
            _traceService.Info($"Found the {sqlScriptFiles.Count} script files on {scriptDirectory}");
            _traceService.Info($"{string.Join(@"\r\n\t", sqlScriptFiles.Select(s => new FileInfo(s).Name))}");

            //execute all script files in the version folder
            sqlScriptFiles.Sort();
            sqlScriptFiles
                .ForEach(scriptFile =>
                {
                    var sqlStatementRaw = _fileService.ReadAllText(scriptFile);
                    var sqlStatements = _dataService.BreakStatements(sqlStatementRaw)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();
                    ;
                    sqlStatements.ForEach(sqlStatement =>
                    {
                        sqlStatement = _tokenReplacementService.Replace(tokenKeyPairs, sqlStatement);

                        _traceService.Debug($"Executing sql statement as part of : {scriptFile}{Environment.NewLine}{sqlStatement}");
                        _configurationDataService.ExecuteSql(
                            connection: connection,
                            commandText: sqlStatement,
                            transaction: transaction,
                            commandTimeout: commandTimeout,
                            traceService: _traceService);
                    });

                    _traceService.Info($"Executed script file {scriptFile}.");
                });
        }
        
        /// <summary>
        /// Executes archive to flatten out all of the existing branches and move into v0.00 again.
        /// </summary>
        /// <param name="workingPath">The directory path to migration project.</param>
        /// <param name="schemaName">Schema name for schema versions table. When empty, uses the default schema in the target data platform. </param>
        /// <param name="tableName">Table name for schema versions table. When empty, uses __yuniqldbversion.</param>
        /// <param name="tokens">Token kev/value pairs to replace tokens in script files.</param>
        /// <param name="appliedByTool">The source that initiates the migration. This can be yuniql-cli, yuniql-aspnetcore or yuniql-azdevops.</param>
        /// <param name="appliedByToolVersion">The version of the source that initiates the migration.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        public void Archive(
          string workingPath,
          int commandTimeout = 0,
          string schemaName = null,
          string tableName = null,
          string appliedByTool = null,
          string appliedByToolVersion = null,
          List<KeyValuePair<string, string>> tokens = null)
        {

          //check existing branch version and move them inside v0.00
          if (!MoveAllExistingBranchToVzero(workingPath))
            return;
          //reset historic version to v0.00 in __yuniqlversion table
          ResetVersion(workingPath, schemaName, tableName, appliedByTool, appliedByToolVersion);

        }

        private bool MoveAllExistingBranchToVzero(string workingPath)
        { 
          string[] directoryList = Directory.GetDirectories(workingPath);
          List<string> branchList = new List<string>();

          //check existing branch version
          string versionPattern = @"^v(?<major>\d+)\.(?<minor>\d\d)(?<label>.*)$";

          _traceService.Info($"Folder structure before archiving: ");
          foreach (string subDirectory in directoryList)
          {
            string curfolderName = new DirectoryInfo(subDirectory).Name;
            Match versionMatch = Regex.Match(curfolderName, versionPattern);
            _traceService.Info($"-{curfolderName}");

            if(versionMatch.Success) 
              branchList.Add(subDirectory);
          }

          if (branchList.Count <= 1) 
          {
            _traceService.Info($"No branch version or it's already archived");
            return false;
          }

          //create temporary folder
          string vtmp = Path.Combine(workingPath, "vtmp");
          string v000 = Path.Combine(workingPath, "v0.00");

          Directory.CreateDirectory(vtmp);

          if (!Directory.Exists(vtmp))  
          { 
            _traceService.Error($"Enable to create temporary script folder {vtmp}");
            return false;
          } 

          _traceService.Info($"Created temporary script folder {workingPath}");

          try
          {
            foreach (string source in branchList)
            {
              string curSourceName = new DirectoryInfo(source).Name;
              //Move all folder vX.XX* version inside vtmp
              Directory.Move(source, Path.Combine(vtmp, curSourceName));
            }
          }
          catch (Exception)
          {
            _traceService.Error("Archiving was not running. Enable to move all folder vX.XX* version inside vtmp.");
            throw;
          }

          try
          {       
            //rename vtmp folder to v0.00
            Directory.Move(vtmp, v000);
          }
          catch (Exception)
          {
            _traceService.Error($"Enable to rename {vtmp} to {v000}");
            _traceService.Error($"Attempt to rename {vtmp} to {v000}");
            Directory.Move(vtmp, v000);
          }
          finally { }

          _traceService.Info($"Branches moved into {v000}");

          _traceService.Info($"Folder structure after archiving: ");

          directoryList = Directory.GetDirectories(workingPath);

          foreach (string curFolder in directoryList)
          {
            string curfolderName = new DirectoryInfo(curFolder).Name;
            Match versionMatch = Regex.Match(curfolderName, versionPattern);
            _traceService.Info($"-{curfolderName}");

            if(versionMatch.Success) 
            {
              string[] subDirectoryList = Directory.GetDirectories(curFolder);

              foreach (string curDir in subDirectoryList)
              {
                string curDirName = new DirectoryInfo(curDir).Name;
                _traceService.Info("\t|");  
                _traceService.Info($"\t+-{curDirName}");    
              }
            }
          }

          return true;
        }

        private void ResetVersion (
          string workingPath,
          string schemaName = null,
          string tableName = null,
          string appliedByTool = null,
          string appliedByToolVersion = null,
          string versionDirectory = null,
          int commandTimeout = 0) {
          //Serialize the existing contents of the table __yuniqldbversion 
          string versions = GetAllVersionsAsJson ();
          _traceService.Debug (@$"version {versions}.");
          //Encode into base64 and as artifact column in the new version v0.00
          string versionsEncoded;
          byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes (versions);
          versionsEncoded = System.Convert.ToBase64String (data);

          _traceService.Debug (@$"version encoded {versionsEncoded}.");

          //Delete all existing versions in __yuniqldbversion table
          ClearAllVersions ();

          //Insert a new version v0.00 in __yuniqlversion table
          //update db version
          string v000 = Path.Combine(workingPath, "v0.00");
          var versionName = versionDirectory == null ? new DirectoryInfo (v000).Name : new DirectoryInfo (versionDirectory).Name;
          var targeDatabaseLatest = IsTargetDatabaseLatest (versionName, schemaName, tableName);
          if (!targeDatabaseLatest) {
            //create a shared open connection to entire migration run
            using (var connection = _dataService.CreateConnection ()) {
              connection.Open ();

              //enclose all executions in a single transaction in case platform supports it
              if (_dataService.IsAtomicDDLSupported) {
                _traceService.Debug (@$"Target platform fully supports transactions. Migration will run in single transaction.");
                using (var transaction = connection.BeginTransaction ()) {
                  try {

                    _configurationDataService.InsertVersionWithArtifact (connection, transaction, versionName,
                      schemaName : schemaName,
                      tableName : tableName,
                      commandTimeout : commandTimeout,
                      appliedByTool : appliedByTool,
                      appliedByToolVersion : appliedByToolVersion,
                      artifactInfo : versionsEncoded);

                    transaction.Commit ();

                  } catch (Exception) {
                    _traceService.Error ("Target database will be rolled back to its previous state.");
                    transaction.Rollback ();
                    throw;
                  }
                }
              } else //otherwise don't use transactions
              {
                try {
                  _traceService.Info ($"Target platform doesn't reliably support transactions for all commands. " +
                    $"Migration will not run in single transaction. " +
                    $"Any failure during the migration can prevent automatic completing of migration.");

                  _configurationDataService.InsertVersionWithArtifact (connection, null, versionName,
                    schemaName : schemaName,
                    tableName : tableName,
                    commandTimeout : commandTimeout,
                    appliedByTool : appliedByTool,
                    appliedByToolVersion : appliedByToolVersion,
                    artifactInfo : versionsEncoded);
                } catch (Exception) {
                  _traceService.Error ("Migration was not running in transaction, therefore roll back of target database to its previous state is not possible. " +
                    "Migration need to be completed manually! Running of Yuniql again, might cause that some scripts will be executed twice!");
                  throw;
                }
              }
            }

            return;
          }
        }
    }
}
