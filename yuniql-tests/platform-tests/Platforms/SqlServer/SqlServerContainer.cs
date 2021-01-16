using System;
using System.Collections.Generic;
using Yuniql.PlatformTests.Infrastructure;

namespace Yuniql.PlatformTests.Platforms.SqlServer
{
    //docker run -dit -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
    public class SqlServerContainer : ContainerBase
    {
        public SqlServerContainer()
        {
            Name = "sqlserver-test-infra";
            Image = "mcr.microsoft.com/mssql/server";
            Tag = "2017-latest";
            Env = new List<Tuple<string, string>> {
                    new Tuple<string, string>("ACCEPT_EULA","Y"),
                    new Tuple<string, string>("MSSQL_SA_PASSWORD","P@ssw0rd!"),
                };
            Cmd = new List<string> { };
            ExposedPorts = new List<string> { "1433" };
            MappedPorts = new List<Tuple<string, string>> {
                   new Tuple<string, string>("1433","1400")
            };
        }
    };
}
