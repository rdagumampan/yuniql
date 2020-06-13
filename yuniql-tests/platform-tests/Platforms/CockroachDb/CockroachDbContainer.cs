using System;
using System.Collections.Generic;

namespace Yuniql.PlatformTests
{
    public class CockroachDbContainer : ContainerBase
    {
        public CockroachDbContainer()
        {
            Name = "cockroachdb-test-infra";
            Image = "cockroachdb/cockroach";
            Tag = "latest";
            Env = new List<Tuple<string, string>> {
            };
            Cmd = new List<string>() { "start --insecure" };
            ExposedPorts = new List<string> { "26257" };
            MappedPorts = new List<Tuple<string, string>> {
                   new Tuple<string, string>("26257","26257"),
                   new Tuple<string, string>("8080","8080"),
            };
        }
    }
}
