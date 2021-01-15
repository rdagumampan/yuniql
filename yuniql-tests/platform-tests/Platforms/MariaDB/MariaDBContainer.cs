using System;
using System.Collections.Generic;
using Yuniql.PlatformTests.Infrastructure;

namespace Yuniql.PlatformTests.Platforms.MariaDB
{
    //docker run -dit --name mariadb -e MYSQL_ROOT_PASSWORD=P@ssw0rd! -d -p 3306:3306 mariadb:latest --default-authentication-plugin=mysql_native_password
    public class MariaDBContainer : ContainerBase
    {
        public MariaDBContainer()
        {
            Name = "mariadb-test-infra";
            Image = "mariadb";
            Tag = "latest";
            Env = new List<Tuple<string, string>> {
                    new Tuple<string, string>("MYSQL_ROOT_PASSWORD","P@ssw0rd!"),
                };
            Cmd = new List<string>() { "--default-authentication-plugin=mysql_native_password" };
            ExposedPorts = new List<string> { "3306" };
            MappedPorts = new List<Tuple<string, string>> {
                   new Tuple<string, string>("3306","3306")
                };
        }
    }
}
