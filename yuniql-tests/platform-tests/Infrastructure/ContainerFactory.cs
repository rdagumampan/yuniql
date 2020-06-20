using System;
using Yuniql.Core;

namespace Yuniql.PlatformTests
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
