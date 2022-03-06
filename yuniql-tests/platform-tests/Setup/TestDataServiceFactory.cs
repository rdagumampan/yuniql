using System;
using Yuniql.Core;
using Yuniql.PlatformTests.Interfaces;
using Yuniql.PlatformTests.Platforms.MySql;
using Yuniql.PlatformTests.Platforms.PostgreSql;
using Yuniql.PlatformTests.Platforms.Redshift;
using Yuniql.PlatformTests.Platforms.Snowflake;
using Yuniql.PlatformTests.Platforms.SqlServer;
using Yuniql.SqlServer;
using Yuniql.PostgreSql;
using Yuniql.MySql;
using Yuniql.Snowflake;
using Yuniql.Redshift;
using Yuniql.Oracle;

namespace Yuniql.PlatformTests.Setup
{
    //https://github.com/dotnet/samples/tree/master/core/tutorials/Unloading
    public class TestDataServiceFactory : ITestDataServiceFactory
    {
        public TestDataServiceFactory()
        {
        }

        public ITestDataService Create(string platform)
        {
            var traceService = new FileTraceService() { IsDebugEnabled = true };
            var directoryService = new DirectoryService(traceService);
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
                case SUPPORTED_DATABASES.SNOWFLAKE:
                    {
                        return new SnowflakeTestDataService(new SnowflakeDataService(traceService), tokenReplacementService);
                    }
                case SUPPORTED_DATABASES.REDSHIFT:
                    {
                        return new RedshiftTestDataService(new RedshiftDataService(traceService), tokenReplacementService);
                    }
                case SUPPORTED_DATABASES.ORACLE:
                    {
                        return new OracleTestDataService(new OracleDataService(traceService), tokenReplacementService);
                    }
                default:
                    throw new NotSupportedException($"The target database platform {platform} is not supported or plugins location was not correctly configured. " +
                        $"See WIKI for supported database platforms and usage guide.");
            }
        }
    }
}

