using System;
using System.Collections.Generic;

namespace Yuniql.PlatformTests.Infrastructure
{
    public class ContainerBase
    {

        public string Id { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }

        public string Tag { get; set; }

        public List<Tuple<string, string>> Env { get; set; } = new List<Tuple<string, string>>();

        public List<string> Cmd { get; set; } = new List<string>();

        public List<string> ExposedPorts { get; set; } = new List<string>();

        public List<Tuple<string, string>> MappedPorts { get; set; } = new List<Tuple<string, string>>();

        public string State { get; set; }
    }
}
