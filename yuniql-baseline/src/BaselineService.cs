using System;
using System.Text;
using System.IO;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Yuniql.Extensions
{
    public class BaselineService
    {
        private List<string> processedUrns = new List<string>();
        public delegate bool FilterDelegate(Urn urn);

        public void Run(string sourceConnectionString, string destinationFullPath)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(sourceConnectionString);

            var serverConnection = CreateServerConnection(sourceConnectionString);
            var server = new Server(serverConnection);
            server.SetDefaultInitFields(true);
            server.ConnectionContext.Connect();

            var scripter = CreateSqlScripter(server);
            var database = server.Databases[connectionStringBuilder.InitialCatalog];

            var schemasDirectory = GetOrCreateDestinationDirectory(destinationFullPath, "01-schemas");
            var schemasUrns = GetGenericUrns(database.Schemas);
            GenerateSchemaBasedScriptFiles(sourceConnectionString, scripter, schemasDirectory, schemasUrns);

            var typeDirectory = GetOrCreateDestinationDirectory(destinationFullPath, "02-types");
            var typeUrns = GetGenericUrns(database.UserDefinedTypes);
            GenerateSchemaBasedScriptFiles(sourceConnectionString, scripter, typeDirectory, typeUrns);

            var dataTypeDirectory = GetOrCreateDestinationDirectory(destinationFullPath, "02-types");
            var dataTypeUrns = GetGenericUrns(database.UserDefinedDataTypes);
            GenerateSchemaBasedScriptFiles(sourceConnectionString, scripter, dataTypeDirectory, dataTypeUrns);

            var tableTypeDirectory = GetOrCreateDestinationDirectory(destinationFullPath, "02-types");
            var tableTypeUrns = GetGenericUrns(database.UserDefinedTableTypes);
            GenerateSchemaBasedScriptFiles(sourceConnectionString, scripter, tableTypeDirectory, tableTypeUrns);

            var xmlschemasDirectory = GetOrCreateDestinationDirectory(destinationFullPath, "03-xmlschemas");
            var xmlschemasUrns = GetGenericUrns(database.XmlSchemaCollections);
            GenerateSchemaBasedScriptFiles(sourceConnectionString, scripter, xmlschemasDirectory, xmlschemasUrns);

            var tableDirectory = GetOrCreateDestinationDirectory(destinationFullPath, "04-tables");
            var tableUrns = GetTableUrns(database, scripter);
            GenerateSchemaBasedScriptFiles(sourceConnectionString, scripter, tableDirectory, tableUrns);

            var viewDirectory = GetOrCreateDestinationDirectory(destinationFullPath, "05-views");
            var viewUrns = GetGenericUrns(database.Views);
            GenerateSchemaBasedScriptFiles(sourceConnectionString, scripter, viewDirectory, viewUrns);

            var functionDirectory = GetOrCreateDestinationDirectory(destinationFullPath, "06-functions");
            var functionUrns = GetGenericUrns(database.UserDefinedFunctions);
            GenerateSchemaBasedScriptFiles(sourceConnectionString, scripter, functionDirectory, functionUrns);

            var procedureDirectory = GetOrCreateDestinationDirectory(destinationFullPath, "07-procedures");
            var procedureUrns = GetGenericUrns(database.StoredProcedures);
            GenerateSchemaBasedScriptFiles(sourceConnectionString, scripter, procedureDirectory, procedureUrns);

            var sequencesDirectory = GetOrCreateDestinationDirectory(destinationFullPath, "08-sequences");
            var sequencesUrns = GetGenericUrns(database.Sequences);
            GenerateSchemaBasedScriptFiles(sourceConnectionString, scripter, sequencesDirectory, sequencesUrns);

            var triggersDirectory = GetOrCreateDestinationDirectory(destinationFullPath, "09-triggers");
            var triggersUrns = GetGenericUrns(database.Triggers);
            GenerateSchemaBasedScriptFiles(sourceConnectionString, scripter, triggersDirectory, triggersUrns);
        }

        public static bool FilterTypes(Urn urn)
        {
            if (urn.Type == "UserDefinedDataType")
            {
                TraceService.Debug("Filtered UserDefinedDataType");
                return true;
            }

            if (urn.Type == "XmlSchemaCollection")
            {
                TraceService.Debug("Filtered XmlSchemaCollection");
                return true;
            }

            if (urn.Type == "StoredProcedure")
            {
                TraceService.Debug("Filtered StoredProcedure");
                return true;
            }

            if (urn.Type == "UnresolvedEntity")
            {
                TraceService.Debug("Filtered UnresolvedEntity");
                return true;
            }

            return false;
        }

        private static void Scripter_DiscoveryProgress(object sender, ProgressReportEventArgs e)
        {
            if (e.Current.Type is UserDefinedDataType)
            {
                TraceService.Info("Discovery skipped UserDefinedDataType");
                e.Current.Value = string.Empty;
            }
        }

        private static void Scripter_ScriptingProgress(object sender, ProgressReportEventArgs e)
        {
            TraceService.Info("---------" + e.Current.Type);
        }

        private void Scripter_ScriptingError(object sender, ScriptingErrorEventArgs e)
        {
            TraceService.Error($"Error scripting {e.Current.Value}. {e.InnerException.ToString()}");
        }

        private ServerConnection CreateServerConnection(string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

            var connection = new ServerConnection();
            connection.ServerInstance = connectionStringBuilder.DataSource;
            connection.LoginSecure = connectionStringBuilder.IntegratedSecurity;
            connection.Login = connectionStringBuilder.UserID;
            connection.Password = connectionStringBuilder.Password;

            return connection;
        }

        private Scripter CreateSqlScripter(Server server)
        {
            var options = new ScriptingOptions();
            options.EnforceScriptingOptions = true;

            //we script only the schema and dependencies are handled separately thru dependency tree walk
            options.ScriptData = false;
            options.ScriptSchema = true;
            options.WithDependencies = false;

            //how results are formatted
            options.IncludeHeaders = false;
            options.IncludeIfNotExists = false;
            options.IncludeScriptingParametersHeader = false;
            options.SchemaQualify = true;
            options.SchemaQualifyForeignKeysReferences = true;
            options.ScriptDrops = false;
            options.ScriptForAlter = false;
            options.ScriptForCreateDrop = false;
            options.ScriptBatchTerminator = true;
            options.NoCommandTerminator = false;
            options.AnsiPadding = true;
            options.AnsiFile = true;
            options.Encoding = Encoding.UTF8;
            options.ToFileOnly = true;
            options.AppendToFile = true;
            options.IncludeDatabaseContext = false;

            //general options for all schema objects
            options.PrimaryObject = true;

            //table specific options
            options.Default = true;
            options.Triggers = true;

            //table indexes specific options
            options.Indexes = true;
            options.ClusteredIndexes = true;
            options.NonClusteredIndexes = true;
            options.ColumnStoreIndexes = false;
            options.SpatialIndexes = false;
            options.XmlIndexes = false;

            //we choose to be selective of what index to script out
            options.DriAll = false;

            options.DriDefaults = true;
            options.DriIndexes = true;
            options.DriClustered = true;
            options.DriNonClustered = true;

            options.DriAllConstraints = false;
            options.DriChecks = true;
            options.DriWithNoCheck = true;

            options.DriAllKeys = false;
            options.DriPrimaryKey = true;
            options.DriForeignKeys = true;
            options.DriUniqueKeys = true;

            //security specific options, exclude security
            options.Permissions = false;
            options.IncludeDatabaseRoleMemberships = false;
            options.LoginSid = false;

            //system object options, exclude system objects
            options.AllowSystemObjects = false;
            options.DriIncludeSystemNames = false;

            //scripted data conversion options, exclude table data
            options.ScriptDataCompression = false;
            options.TimestampToBinary = false;
            options.ConvertUserDefinedDataTypesToBaseType = false;
            options.Bindings = true;
            options.ChangeTracking = false;

            //agents and jobs specific options, exclude agents
            options.AgentNotify = false;
            options.AgentJobId = false;
            options.AgentAlertJob = false;

            //full text search specific options, exclude fulltext
            options.FullTextStopLists = false;
            options.FullTextIndexes = false;
            options.FullTextCatalogs = false;
            options.IncludeFullTextCatalogRootPath = false;

            options.ExtendedProperties = true;
            options.DdlBodyOnly = false;
            options.DdlHeaderOnly = false;

            options.NoVardecimal = true;
            options.NoMailProfilePrincipals = true;
            options.NoMailProfileAccounts = true;

            //storage specific options, exclude storage settings
            options.NoTablePartitioningSchemes = true;
            options.NoIndexPartitioningSchemes = true;
            options.NoXmlNamespaces = false;
            options.NoCollation = true;
            options.NoFileStreamColumn = true;
            options.NoFileStream = true;
            options.NoFileGroup = true;
            options.NoIdentities = false;
            options.NoAssemblies = true;
            options.NoViewColumns = false;

            //sql server platform specific options
            //options.TargetDatabaseEngineEdition = false;
            //options.TargetDatabaseEngineType = false;
            //options.SqlServerVersion TargetServerVersion = true;

            //others
            options.NoExecuteAs = false;
            //options.BatchSize = 0;
            options.OptimizerData = false;
            options.Statistics = false;
            options.ScriptOwner = false;

            //error handling specific options
            options.ContinueScriptingOnError = false;

            var scripter = new Scripter(server);
            scripter.FilterCallbackFunction = FilterTypes;
            scripter.Options = options;
            scripter.ScriptingError += Scripter_ScriptingError;

            return scripter;
        }

        private List<Urn> GetGenericUrns(SmoCollectionBase collections)
        {
            var urns = new List<Urn>();
            foreach (SqlSmoObject smo in collections)
            {
                //skip all system objects
                if (smo is Schema && ((smo as Schema).IsSystemObject)) continue;
                if (smo is Table && ((smo as Table).IsSystemObject)) continue;
                if (smo is View && ((smo as View).IsSystemObject)) continue;
                if (smo is StoredProcedure && ((smo as StoredProcedure).IsSystemObject)) continue;
                if (smo is UserDefinedFunction && ((smo as UserDefinedFunction).IsSystemObject)) continue;

                TraceService.Debug($"Listed urn for scripting: {smo.Urn}");
                urns.Add(smo.Urn);
            }
            return urns;
        }

        private List<Urn> GetTableUrns(Database database, Scripter scripter)
        {
            //prepare root list of urns to walk through
            var parentUrns = new List<Urn>();
            foreach (Table table in database.Tables)
            {
                if (!table.IsSystemObject) parentUrns.Add(table.Urn);
            }

            //collects dependency urns in the right order
            var dependencyTree = scripter.DiscoverDependencies(parentUrns.ToArray(), true);
            var dependencies = scripter.WalkDependencies(dependencyTree);

            var dependencyUrns = new List<Urn>();
            foreach (DependencyCollectionNode dependency in dependencies)
            {
                dependencyUrns.Add(dependency.Urn);
            }

            return dependencyUrns;
        }

        private void GenerateSchemaBasedScriptFiles(string connectionString, Scripter scripter, string destinationDirectory, List<Urn> urns)
        {
            var serverConnection = CreateServerConnection(connectionString);
            var server = new Server(serverConnection);

            try
            {
                server.ConnectionContext.Connect();

                var sequenceNo = 1;
                foreach (var urn in urns)
                {
                    if (processedUrns.Contains(urn)) continue;

                    var smo = server.GetSmoObject(urn) as ScriptNameObjectBase;
                    if (null != smo)
                    {
                        var baseFileName = $"{smo.Name}";

                        if (smo is ScriptSchemaObjectBase)
                        {
                            var ssmo = smo as ScriptSchemaObjectBase;
                            if (!string.IsNullOrEmpty(ssmo.Schema))
                            {
                                baseFileName = $"{ssmo.Schema}.{ssmo.Name}";
                            }
                        }

                        scripter.Options.FileName = Path.Combine(destinationDirectory, $"{sequenceNo.ToString("000")}-{baseFileName}.sql");
                        scripter.Script(new Urn[] { urn });

                        processedUrns.Add(urn);
                        sequenceNo++;

                        TraceService.Info($"Generated script file {scripter.Options.FileName}");
                    }
                    else
                    {
                        TraceService.Error($"Failed to generate scripts for urn: {urn}");
                    }
                }
            }
            catch (Exception ex)
            {
                TraceService.Error($"Error generating schema files. {ex.ToString()}");
                throw;
            }
            finally
            {
                if (server.ConnectionContext.IsOpen)
                {
                    server.ConnectionContext.Disconnect();
                    server = null;
                }
            }
        }

        private string GetOrCreateDestinationDirectory(string workingPath, string folder)
        {
            var tableDirectory = Path.Combine(workingPath, folder);
            if (!Directory.Exists(tableDirectory))
            {
                Directory.CreateDirectory(tableDirectory);
            }

            return tableDirectory;
        }
    }
}
