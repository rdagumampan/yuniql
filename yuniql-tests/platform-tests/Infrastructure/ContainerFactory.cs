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
                SUPPORTED_DATABASES.POSTGRESQL => new MySqlContainer(),
                SUPPORTED_DATABASES.MYSQL => new MySqlContainer(),
                SUPPORTED_DATABASES.MARIADB => new MariaDBContainer(),
                SUPPORTED_DATABASES.COCKROACHDB => new CockroachDbContainer(),
                _ => throw new NotSupportedException()                
                ,
            };
        }
    }
}
