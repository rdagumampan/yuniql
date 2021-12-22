using System;
using Yuniql.Core;
using Yuniql.Extensibility;
using Yuniql.MySql;
using Yuniql.Oracle;
using Yuniql.PostgreSql;
using Yuniql.Redshift;
using Yuniql.Snowflake;
using Yuniql.SqlServer;

namespace Yuniql.CLI
{
    public class DataServiceFactory : IDataServiceFactory
    {
        private readonly ITraceService _traceService;

        public DataServiceFactory(
            ITraceService traceService)
        {
            this._traceService = traceService;
        }

        public IDataService Create(string platform)
        {
            switch (platform.ToLower())
            {
                case SUPPORTED_DATABASES.SQLSERVER:
                    {
                        return new SqlServerDataService(_traceService);
                    }
                case SUPPORTED_DATABASES.POSTGRESQL:
                    {
                        return new PostgreSqlDataService(_traceService);
                    }
                case SUPPORTED_DATABASES.MYSQL:
                    {
                        return new MySqlDataService(_traceService);
                    }
                case SUPPORTED_DATABASES.MARIADB:
                    {
                        return new MySqlDataService(_traceService);
                    }
                case SUPPORTED_DATABASES.SNOWFLAKE:
                    {
                        return new SnowflakeDataService(_traceService);
                    }
                case SUPPORTED_DATABASES.REDSHIFT:
                    {
                        return new RedshiftDataService(_traceService);
                    }
                case SUPPORTED_DATABASES.ORACLE:
                    {
                        return new OracleDataService(_traceService);
                    }
                default:
                    throw new NotSupportedException($"The target database platform {platform} is not supported or plugins location was not correctly configured. " +
                        $"See WIKI for supported database platforms and usage guide.");
            }
        }
    }
}

