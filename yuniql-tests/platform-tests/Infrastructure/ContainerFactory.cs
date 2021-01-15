using System;
using Yuniql.Core;
using Yuniql.PlatformTests.Platforms.MariaDB;
using Yuniql.PlatformTests.Platforms.MySql;
using Yuniql.PlatformTests.Platforms.PostgreSql;
using Yuniql.PlatformTests.Platforms.SqlServer;

namespace Yuniql.PlatformTests.Infrastructure
{
    public class ContainerFactory
    {
        public ContainerBase Create(string platform)
        {
            return platform switch
            {
                SUPPORTED_DATABASES.SQLSERVER => new SqlServerContainer(),
                SUPPORTED_DATABASES.POSTGRESQL => new PostgreSqlContainer(),
                SUPPORTED_DATABASES.MYSQL => new MySqlContainer(),
                SUPPORTED_DATABASES.MARIADB => new MariaDBContainer(),
                _ => throw new NotSupportedException(),
            };
        }
    }
}
