using System;
using Yuniql.Core;
using Yuniql.MySql;
using Yuniql.PostgreSql;
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
            var traceService = new FileTraceService { IsDebugEnabled = true };
            var tokenReplacementService = new TokenReplacementService(traceService);

            switch (platform.ToLower())
            {
                case SUPPORTED_DATABASES.SQLSERVER:
                    {
                        return new SqlServerTestDataService(new SqlServerDataService(traceService), tokenReplacementService);
                    }
                case SUPPORTED_DATABASES.POSTGRESQL:
                    {
                        return new PostgreSqlTestDataService(new PostgreSqlDataService(traceService), tokenReplacementService);
                    }
                case SUPPORTED_DATABASES.MYSQL:
                    {
                        return new MySqlTestDataService(new MySqlDataService(traceService), tokenReplacementService);
                    }
                case SUPPORTED_DATABASES.MARIADB:
                    {
                        return new MySqlTestDataService(new MySqlDataService(traceService), tokenReplacementService);
                    }
                default:
                    throw new NotSupportedException($"The target database platform {platform} is not supported or plugins location was not correctly configured. " +
                        $"See WIKI for supported database platforms and usage guide.");
            }
        }
    }
}

