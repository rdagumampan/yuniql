using System;
using System.Text;
using System.IO;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using System.Collections.Generic;

//https://gist.github.com/vincpa/1755925
//https://www.sqlservermigrations.com/2018/08/script-databases-and-objects-using-powershell/
//https://www.sqlservercentral.com/wp-content/uploads/2019/05/ALZDBA_ScriptDb_batch.ps1.txt

//scripting with powershell core
//https://github.com/microsoft/mssql-scripter
//https://docs.microsoft.com/en-us/sql/ssdt/download-sql-server-data-tools-ssdt?view=sql-server-ver15
//https://gist.github.com/vincpa/1755925
//https://docs.microsoft.com/en-us/sql/relational-databases/server-management-objects-smo/tasks/scripting?view=sql-server-ver15

//smo in linux
//http://www.maxtblog.com/2017/06/using-linux-sql-server-smo-in-powershell-core/
//https://www.sqlservermigrations.com/2018/08/script-databases-and-objects-using-powershell/

//http://patlau.blogspot.com/2012/09/generate-sqlserver-scripts-with.html
//https://github.com/commercehub-oss/scriptdb

namespace Baseliner
{
    public class Program
    {
        static void Main(string[] args)
        {
            var baselineService = new BaselineService();
            baselineService.Run();
        }

    }

    public class BaselineService
    {
        public delegate bool FilterDelegate(Urn urn);
        public static bool FilterMethod(Urn urn)
        {
            if (urn.Type == "StoredProcedure")
            {
                Console.WriteLine("Filtered StoredProcedure");
                return true;
            }

            if (urn.Type == "UserDefinedDataType")
            {
                Console.WriteLine("Filtered UserDefinedDataType");
                return true;
            }


            if (urn.Type == "UnresolvedEntity")
            {
                Console.WriteLine("Filtered UnresolvedEntity");
                return true;
            }

            if (urn.Type == "XmlSchemaCollection")
            {
                Console.WriteLine("Filtered XmlSchemaCollection");
                return true;
            }

            if (urn.Type == "UserDefinedFunction")
            {
                Console.WriteLine("Filtered UserDefinedFunction");
                return true;
            }

            return false;
        }

        private static void Scripter_DiscoveryProgress(object sender, ProgressReportEventArgs e)
        {
            if (e.Current.Type is UserDefinedDataType)
                e.Current.Value = string.Empty;
        }

        private static void Scripter_ScriptingProgress(object sender, ProgressReportEventArgs e)
        {
            Console.WriteLine("---------" + e.Current.Type);
        }

        private Server server;
        private Database database;
        public void Init()
        {
            var connection = new ServerConnection(new SqlConnectionInfo(Environment.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING")));
            connection.Connect();

            server = new Server(connection);
            database = server.Databases["AdventureWorks"];

        }
        public Scripter CreateScripter()
        {
            var options = new ScriptingOptions();
            options.EnforceScriptingOptions = true;

            //we script only the schema and their dependencies
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

            //general options
            options.PrimaryObject = true;

            //table specific options
            options.Default = true;
            options.Triggers = false;

            //table indexes specific options
            options.Indexes = true;
            options.XmlIndexes = false;
            options.ClusteredIndexes = true;
            options.NonClusteredIndexes = true;
            options.ColumnStoreIndexes = false;
            options.SpatialIndexes = false;

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

            //security specific options 
            options.Permissions = false;
            options.IncludeDatabaseRoleMemberships = false;
            options.LoginSid = false;

            //system object options
            options.AllowSystemObjects = false;
            options.DriIncludeSystemNames = false;

            //scripted data conversion options
            options.ScriptDataCompression = false;
            options.TimestampToBinary = false;
            options.ConvertUserDefinedDataTypesToBaseType = false;
            options.Bindings = true;
            options.ChangeTracking = false;

            //agents and jobs specific options
            options.AgentNotify = false;
            options.AgentJobId = false;
            options.AgentAlertJob = false;

            //full text search specific options
            options.FullTextStopLists = false;
            options.FullTextIndexes = false;
            options.FullTextCatalogs = false;
            options.IncludeFullTextCatalogRootPath = false;

            options.ExtendedProperties = false;
            options.DdlBodyOnly = false;
            options.DdlHeaderOnly = false;

            options.NoVardecimal = true;
            options.NoMailProfilePrincipals = true;
            options.NoMailProfileAccounts = true;
            options.NoTablePartitioningSchemes = true;
            options.NoIndexPartitioningSchemes = true;
            options.NoXmlNamespaces = true;
            options.NoCollation = true;
            options.NoFileStreamColumn = true;
            options.NoFileStream = true;
            options.NoFileGroup = true;
            options.NoIdentities = true;
            options.NoAssemblies = true;
            options.NoViewColumns = true;

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
            scripter.FilterCallbackFunction = FilterMethod;
            scripter.Options = options;

            return scripter;
        }
        public void Run()
        {
            Init();
            var scripter = CreateScripter();

            var schemasDirectory = GetDropFolder("01-schemas");
            var schemasUrns = GetUrns(database.XmlSchemaCollections);
            GenerateSchemaBasedScripts(scripter, schemasDirectory, schemasUrns);

            var typeDirectory = GetDropFolder("02-types");
            var typeUrns = GetUrns(database.UserDefinedTypes);
            GenerateSchemaBasedScripts(scripter, typeDirectory, typeUrns);

            var dataTypeDirectory = GetDropFolder("02-types");
            var dataTypeUrns = GetUrns(database.UserDefinedDataTypes);
            GenerateSchemaBasedScripts(scripter, dataTypeDirectory, dataTypeUrns);

            var tableTypeDirectory = GetDropFolder("02-types");
            var tableTypeUrns = GetUrns(database.UserDefinedTableTypes);
            GenerateSchemaBasedScripts(scripter, tableTypeDirectory, tableTypeUrns);

            var xmlschemasDirectory = GetDropFolder("03-xmlschemas");
            var xmlschemasUrns = GetUrns(database.XmlSchemaCollections);
            GenerateSchemaBasedScripts(scripter, xmlschemasDirectory, xmlschemasUrns);

            var tableDirectory = GetDropFolder("04-tables");
            var tableUrns = GetTableUrns(scripter);
            GenerateSchemaBasedScripts(scripter, tableDirectory, tableUrns);

            var viewDirectory = GetDropFolder("05-views");
            var viewUrns = GetUrns(database.Views);
            GenerateSchemaBasedScripts(scripter, viewDirectory, viewUrns);

            var procedureDirectory = GetDropFolder("06-procedures");
            var procedureUrns = GetUrns(database.StoredProcedures);
            GenerateSchemaBasedScripts(scripter, procedureDirectory, procedureUrns);

            var functionDirectory = GetDropFolder("07-functions");
            var functionUrns = GetUrns(database.UserDefinedFunctions);
            GenerateSchemaBasedScripts(scripter, functionDirectory, functionUrns);

            var sequencesDirectory = GetDropFolder("08-sequences");
            var sequencesUrns = GetUrns(database.Sequences);
            GenerateSchemaBasedScripts(scripter, sequencesDirectory, sequencesUrns);

            var triggersDirectory = GetDropFolder("09-triggers");
            var triggersUrns = GetUrns(database.Triggers);
            GenerateSchemaBasedScripts(scripter, triggersDirectory, triggersUrns);
        }

        public List<Urn> GetUrns(SmoCollectionBase collections)
        {
            var urns = new List<Urn>();
            foreach (SqlSmoObject smo in collections)
            {
                if (smo is Table && ((smo as Table).IsSystemObject)) continue;
                if (smo is View && ((smo as View).IsSystemObject)) continue;
                if (smo is UserDefinedFunction && ((smo as UserDefinedFunction).IsSystemObject)) continue;
                if (smo is StoredProcedure && ((smo as StoredProcedure).IsSystemObject)) continue;

                urns.Add(smo.Urn);
            }
            return urns;
        }

        public List<Urn> GetTableUrns(Scripter scripter)
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

        public void GenerateSchemaBasedScripts(Scripter scripter, string destinationDirectory, List<Urn> urns)
        {
            var sequenceNo = 1;
            foreach (var urn in urns)
            {
                var smo = server.GetSmoObject(urn) as ScriptNameObjectBase;
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
                sequenceNo++;

                Console.WriteLine($"OK {scripter.Options.FileName}");
            }
        }

        public string GetDropFolder(string folder)
        {
            var workingPath = Environment.CurrentDirectory;
            var tableDirectory = Path.Combine(workingPath, folder);
            if (!Directory.Exists(tableDirectory))
            {
                Directory.CreateDirectory(tableDirectory);
            }

            return tableDirectory;
        }
    }
}
