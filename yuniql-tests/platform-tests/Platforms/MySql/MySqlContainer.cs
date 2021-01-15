using System;
using System.Collections.Generic;
using Yuniql.PlatformTests.Infrastructure;

namespace Yuniql.PlatformTests.Platforms.MySql
{
    //docker run -dit --name mysql -e MYSQL_ROOT_PASSWORD=P@ssw0rd! -d -p 3306:3306 mysql:latest --default-authentication-plugin=mysql_native_password
    public class MySqlContainer : ContainerBase
    {
        public MySqlContainer()
        {
            Name = "mysql-test-infra";
            Image = "mysql";
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
