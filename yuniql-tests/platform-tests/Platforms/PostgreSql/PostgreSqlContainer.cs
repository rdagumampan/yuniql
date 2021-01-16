using System;
using System.Collections.Generic;
using Yuniql.PlatformTests.Infrastructure;

namespace Yuniql.PlatformTests.Platforms.PostgreSql
{
    //docker run -dit --name postgresql -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=yuniqldb -p 5432:5432 postgres
    public class PostgreSqlContainer : ContainerBase
    {
        public PostgreSqlContainer()
        {
            Name = "postgresql-test-infra";
            Image = "postgres";
            Tag = "latest";
            Env = new List<Tuple<string, string>> {
                    new Tuple<string, string>("POSTGRES_USER","sa"),
                    new Tuple<string, string>("POSTGRES_PASSWORD","P@ssw0rd!"),
                    new Tuple<string, string>("POSTGRES_DB","yuniqldb")
                };
            Cmd = new List<string> { };
            ExposedPorts = new List<string> { "5432" };
            MappedPorts = new List<Tuple<string, string>> {
                   new Tuple<string, string>("5432","5432")
            };
        }
    }
}
