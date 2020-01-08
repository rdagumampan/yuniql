using System;
using System.Collections.Generic;

namespace Yuniql.CliTests
{
    public class PostgreSqlContainer : DockerContainer
    {
        public PostgreSqlContainer()
        {
            Name = "postgresql-testserver-for-cli-tests";
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
