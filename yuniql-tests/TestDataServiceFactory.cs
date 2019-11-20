using System;
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
            else if (platform.Equals("pgsql"))
            {
                var type = typeof(IDataService);
                var assembly = Assembly.LoadFrom(@"C:\play\yuniql\yuniql-plugins\postgresql\src\bin\Release\netcoreapp3.0\win-x64\publish\Yuniql.PostgreSql.dll");

                var dataService = assembly.GetTypes()
                    .Where(t=> t.Name.Contains("PostgreSqlTestDataService"))
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

