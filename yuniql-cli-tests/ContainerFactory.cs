using System;
using Yuniql.CliTests;

namespace CliTests
{
    public class ContainerFactory
    {
        public DockerContainer Create(string platform)
        {
            if (platform.Equals("sqlserver"))
            {
                return new SqlServerContainer();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
