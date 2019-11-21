using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Yuniql.Extensibility;
using Yuniql.SqlServer;

namespace Yuniql.Tests
{
    public class TestDataServiceFactory : ITestDataServiceFactory
    {
        public TestDataServiceFactory()
        {
        }

        public ITestDataService Create(string platform)
        {
            if (platform.Equals("sqlserver"))
            {
                var dataService = new SqlServerTestDataService();
                return dataService;
            }
            else
            {
                //extracts plugins and creates required services
                var assemblyFile = Path.Combine(Environment.CurrentDirectory, ".plugins", platform, $"Yuniql.{platform}.dll");
                if (File.Exists(assemblyFile))
                {
                    var assembly = Assembly.LoadFrom(assemblyFile);
                    var dataService = assembly.GetTypes()
                    .Where(t => t.Name.ToLower().Contains($"{platform}testdataservice"))
                    .Select(t => Activator.CreateInstance(t))
                    .Cast<ITestDataService>()
                    .First();
                    return dataService;
                }
                else
                {
                    throw new NotSupportedException($"The target database platform {platform} is not yet supported. See WIKI for supported database platforms.");
                }
            }
        }
    }
}

