using System;
using System.Collections.Generic;
using System.Text;

namespace Yuniql.CliTests
{
    public class Bootstrapper
    {
        public void Initialize()
        {
            var container = new DockerContainer
            {
                Name = "sqlserver-testdb-cli",
                Image = "mcr.microsoft.com/mssql/server",
                Tag = "2017-latest",
                Env = new List<Tuple<string, string>> {
                    new Tuple<string, string>("ACCEPT_EULA","Y"),
                    new Tuple<string, string>("MSSQL_SA_PASSWORD","P@ssw0rd!"),
                },
                Cmd = new List<Tuple<string, string>> { },
                ExposedPorts = new List<string> { "1433" },
                MappedPorts = new List<Tuple<string, string>> {
                   new Tuple<string, string>("1433","1400")
                }
            };

            var dockerService = new DockerService();
            dockerService.Initialize();
            dockerService.Run(container);
        }
    }
}
