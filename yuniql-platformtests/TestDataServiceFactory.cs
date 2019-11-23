using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Yuniql.Core;
using Yuniql.Extensibility;
using Yuniql.SqlServer;

namespace Yuniql.PlatformTests
{
    public class TestDataServiceFactory : ITestDataServiceFactory
    {
        public TestDataServiceFactory()
        {
        }

        public ITestDataService Create(string platform)
        {
            var traceService = new TraceService();
            if (platform.Equals("sqlserver"))
            {
                var sqlataService = new SqlServerDataService(traceService);
                var testDataService = new SqlServerTestDataService(sqlataService);
                return testDataService;
            }
            else
            {
                //extracts plugins and creates required services
                var assemblyFile = Path.Combine(Environment.CurrentDirectory, ".plugins", platform, $"Yuniql.{platform}.dll");
                var assemblyBasePath = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_PLUGINS");
                if (!string.IsNullOrEmpty(assemblyBasePath))
                {
                    assemblyFile = Path.Combine(assemblyBasePath, $"Yuniql.{platform}.dll");
                }

                if (File.Exists(assemblyFile))
                {
                    var assembly = Assembly.LoadFrom(assemblyFile);

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

