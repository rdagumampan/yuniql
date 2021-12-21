using System;
using Yuniql.Core;
using Yuniql.Extensibility;
using Yuniql.MySql;
using Yuniql.PostgreSql;
using Yuniql.Redshift;
using Yuniql.Snowflake;
using Yuniql.SqlServer;

namespace Yuniql.CLI
{
    public class ManifestFactory : IManifestFactory
    {
        private readonly ITraceService _traceService;

        public ManifestFactory(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        public ManifestData Create(string platform)
        {
            switch (platform.ToLower())
            {
                case SUPPORTED_DATABASES.SQLSERVER:
                    {
                        return new ManifestData
                        {
                            Name = "SqlServer | Released:",
                            Usage = "yuniql run -a -c <your-connection-string> --platform sqlserver",
                            DocumentationUrl = "https://yuniql.io/docs/get-started-sqlserver/",
                            SamplesUrl = "https://github.com/rdagumampan/yuniql/tree/master/samples/basic-sqlserver-sample"
                        };
                    }
                case SUPPORTED_DATABASES.POSTGRESQL:
                    {
                        return new ManifestData
                        {
                            Name = "PostgreSql | Released:",
                            Usage = "yuniql run -a -c <your-connection-string> --platform postgresql",
                            DocumentationUrl = "https://yuniql.io/docs/get-started-postgresql/",
                            SamplesUrl = "https://github.com/rdagumampan/yuniql/tree/master/samples/basic-postgresql-sample"
                        };
                    }
                case SUPPORTED_DATABASES.MYSQL:
                    {
                        return new ManifestData
                        {
                            Name = "MySql | Released:",
                            Usage = "yuniql run -a -c <your-connection-string> --platform mysql",
                            DocumentationUrl = "https://yuniql.io/docs/get-started-mysql/",
                            SamplesUrl = "https://github.com/rdagumampan/yuniql/tree/master/samples/basic-mysql-sample"
                        };
                    }
                case SUPPORTED_DATABASES.MARIADB:
                    {
                        return new ManifestData
                        {
                            Name = "MariaDb | Released:",
                            Usage = "yuniql run -a -c <your-connection-string> --platform mariadb",
                            DocumentationUrl = "https://yuniql.io/docs/get-started-mysql/",
                            SamplesUrl = "https://github.com/rdagumampan/yuniql/tree/master/samples/basic-mysql-sample"
                        };
                    }
                case SUPPORTED_DATABASES.SNOWFLAKE:
                    {
                        return new ManifestData
                        {
                            Name = "Snowflake | Released:",
                            Usage = "yuniql run -a -c <your-connection-string> --platform snowflake",
                            DocumentationUrl = "https://yuniql.io/docs/get-started-snowflake/",
                            SamplesUrl = "https://github.com/rdagumampan/yuniql/tree/master/samples/basic-snowflake-sample"
                        };
                    }
                case SUPPORTED_DATABASES.REDSHIFT:
                    {
                        return new ManifestData
                        {
                            Name = "Redshift| Released:",
                            Usage = "yuniql run -a -c <your-connection-string> --platform redshift",
                            DocumentationUrl = "https://yuniql.io/docs/get-started-resdshift/",
                            SamplesUrl = "https://github.com/rdagumampan/yuniql/tree/master/samples/basic-redshift-sample"
                        };
                    }
                case SUPPORTED_DATABASES.ORACLE:
                    {
                        return new ManifestData
                        {
                            Name = "Oracle| Alpha:",
                            Usage = "yuniql run -a -c <your-connection-string> --platform oracle",
                            DocumentationUrl = "https://yuniql.io/docs/get-started-oracle/",
                            SamplesUrl = "https://github.com/rdagumampan/yuniql/tree/master/samples/basic-oracle-sample"
                        };
                    }
                default:
                    throw new NotSupportedException($"The target database platform {platform} is not supported or plugins location was not correctly configured. " +
                        $"See WIKI for supported database platforms and usage guide.");
            }
        }
    }
}

