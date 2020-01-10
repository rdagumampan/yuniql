using System;

namespace Yuniql.PlatformTests
{
    public class ContainerFactory
    {
        public ContainerBase Create(string platform)
        {
            return platform switch
            {
                "sqlserver" => new SqlServerContainer(),
                "postgresql" => new PostgreSqlContainer(),
                "mysql" => new MySqlContainer(),
                _ => throw new NotSupportedException(),
            };
        }
    }
}
