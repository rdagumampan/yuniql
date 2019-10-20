using System;
using System.Text;
using System.IO;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using System.Collections.Generic;
using System.Data.SqlClient;

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
        private List<string> processedUrns = new List<string>();

        public delegate bool FilterDelegate(Urn urn);
        public static bool FilterMethod(Urn urn)
        {
            if (urn.Type == "UserDefinedDataType")
            {
                Console.WriteLine("Filtered UserDefinedDataType");
                return true;
            }

            //if (urn.Type == "UserDefinedFunction")
            //{
            //    Console.WriteLine("Filtered UserDefinedFunction");
            //    return true;
            //}

            if (urn.Type == "XmlSchemaCollection")
            {
                Console.WriteLine("Filtered XmlSchemaCollection");
                return true;
            }

            if (urn.Type == "StoredProcedure")
            {
                Console.WriteLine("Filtered StoredProcedure");
                return true;
            }

            if (urn.Type == "UnresolvedEntity")
            {
                Console.WriteLine("Filtered UnresolvedEntity");
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
            var connectionString = Environment.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                //use this when running against local instance of sql server with integrated security
                //connectionString = $"Data Source=.;Integrated Security=SSPI;Initial Catalog=AdventureWorks";
                connectionString = $"Server=.;Database=AdventureWorksLT2016;User Id=sa;Password=P@ssw0rd!";
                //connectionString = $"Server=.;Database=AdventureWorks;User Id=sa;Password=P@ssw0rd!";
            }

            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var connection = new ServerConnection();
            connection.ServerInstance = connectionStringBuilder.DataSource;
            connection.LoginSecure = connectionStringBuilder.IntegratedSecurity;
            connection.Login = connectionStringBuilder.UserID;
            connection.Password = connectionStringBuilder.Password;
            connection.Connect();

            server = new Server(connection);
            database = server.Databases["AdventureWorksLT2016"];
            //database = server.Databases["AdventureWorks"];
        }
        public Scripter CreateScripter()
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
            options.Triggers = false;

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

            options.ExtendedProperties = false;
            options.DdlBodyOnly = false;
            options.DdlHeaderOnly = false;

            options.NoVardecimal = true;
            options.NoMailProfilePrincipals = true;
            options.NoMailProfileAccounts = true;

            //storage specific options, exclude storage settings
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
            var schemasUrns = GetGenericUrns(database.Schemas);
            GenerateSchemaBasedScripts(scripter, schemasDirectory, schemasUrns);

            var typeDirectory = GetDropFolder("02-types");
            var typeUrns = GetGenericUrns(database.UserDefinedTypes);
            GenerateSchemaBasedScripts(scripter, typeDirectory, typeUrns);

            var dataTypeDirectory = GetDropFolder("02-types");
            var dataTypeUrns = GetGenericUrns(database.UserDefinedDataTypes);
            GenerateSchemaBasedScripts(scripter, dataTypeDirectory, dataTypeUrns);

            var tableTypeDirectory = GetDropFolder("02-types");
            var tableTypeUrns = GetGenericUrns(database.UserDefinedTableTypes);
            GenerateSchemaBasedScripts(scripter, tableTypeDirectory, tableTypeUrns);

            var xmlschemasDirectory = GetDropFolder("03-xmlschemas");
            var xmlschemasUrns = GetGenericUrns(database.XmlSchemaCollections);
            GenerateSchemaBasedScripts(scripter, xmlschemasDirectory, xmlschemasUrns);

            var tableDirectory = GetDropFolder("04-tables");
            var tableUrns = GetTableUrns(scripter);
            GenerateSchemaBasedScripts(scripter, tableDirectory, tableUrns);

            var viewDirectory = GetDropFolder("05-views");
            var viewUrns = GetGenericUrns(database.Views);
            GenerateSchemaBasedScripts(scripter, viewDirectory, viewUrns);

            var functionDirectory = GetDropFolder("06-functions");
            var functionUrns = GetGenericUrns(database.UserDefinedFunctions);
            GenerateSchemaBasedScripts(scripter, functionDirectory, functionUrns);

            var procedureDirectory = GetDropFolder("07-procedures");
            var procedureUrns = GetGenericUrns(database.StoredProcedures);
            GenerateSchemaBasedScripts(scripter, procedureDirectory, procedureUrns);

            var sequencesDirectory = GetDropFolder("08-sequences");
            var sequencesUrns = GetGenericUrns(database.Sequences);
            GenerateSchemaBasedScripts(scripter, sequencesDirectory, sequencesUrns);

            var triggersDirectory = GetDropFolder("09-triggers");
            var triggersUrns = GetGenericUrns(database.Triggers);
            GenerateSchemaBasedScripts(scripter, triggersDirectory, triggersUrns);
        }

        public List<Urn> GetGenericUrns(SmoCollectionBase collections)
        {
            var urns = new List<Urn>();
            foreach (SqlSmoObject smo in collections)
            {
                if (smo is Schema && ((smo as Schema).IsSystemObject)) continue;
                if (smo is Table && ((smo as Table).IsSystemObject)) continue;
                if (smo is View && ((smo as View).IsSystemObject)) continue;
                if (smo is StoredProcedure && ((smo as StoredProcedure).IsSystemObject)) continue;
                if (smo is UserDefinedFunction && ((smo as UserDefinedFunction).IsSystemObject)) continue;

                Console.WriteLine($"GetUrns: {smo.Urn}");
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

                    Console.WriteLine($"OK {scripter.Options.FileName}");
                }
                else
                {
                    Console.WriteLine($"Failed to generate scripts for urn: {urn}");
                }
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
