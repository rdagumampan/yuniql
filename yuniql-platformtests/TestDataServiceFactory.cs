using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Yuniql.Core;
using Yuniql.Extensibility;
using Yuniql.SqlServer;

namespace Yuniql.PlatformTests
{
    //https://github.com/dotnet/samples/tree/master/core/tutorials/Unloading
    public class TestDataServiceFactory : ITestDataServiceFactory
    {
        public TestDataServiceFactory()
        {
        }

        public ITestDataService Create(string platform)
        {
            var traceService = new FileTraceService();
            if (platform.Equals("sqlserver"))
            {
                var sqlataService = new SqlServerDataService(traceService);
                var testDataService = new SqlServerTestDataService(sqlataService);
                return testDataService;
            }
            else
            {
                var assemblyBasePath = EnvironmentHelper.GetEnvironmentVariable(EnvironmentVariableNames.YUNIQL_TEST_PLUGINS);
                var assemblyFilePath = Path.Combine(assemblyBasePath, platform, $"Yuniql.{platform}.dll");

                if (File.Exists(assemblyFilePath))
                {
                    var assembly = Assembly.LoadFrom(assemblyFilePath);

                    var sqlDataService = assembly.GetTypes()
                        .Where(t => t.Name.ToLower().Contains($"{platform}dataservice"))
                        .Select(t => Activator.CreateInstance(t, traceService))
                        .Cast<IDataService>()
                        .First();

                    var testDataService = assembly.GetTypes()
                    .Where(t => t.Name.ToLower().Contains($"{platform}testdataservice"))
                    .Select(t => Activator.CreateInstance(t, sqlDataService))
                    .Cast<ITestDataService>()
                    .First();
                    return testDataService;
                }
                else
                {
                    throw new NotSupportedException($"The target database platform {platform} is not yet supported. See WIKI for supported database platforms.");
                }
            }
        }
    }
}

