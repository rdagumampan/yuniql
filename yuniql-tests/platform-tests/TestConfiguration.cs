using Yuniql.Core;

namespace Yuniql.PlatformTests
{
    public class TestConfiguration
    {
        public string CliProcessPath { get; set; }

        public string WorkspacePath { get; set; }

        public string Platform { get; set; }

        public string DatabaseName { get; set; }

        public string ConnectionString { get; set; }

        public string PluginsPath { get; set; }

        public string TestAgentHost { get; set; }

        public Configuration GetConfiguration()
        {
            return new Configuration
            {
                WorkspacePath = this.WorkspacePath,
                Platform = this.Platform,
                ConnectionString = this.ConnectionString,
                AutoCreateDatabase = true
            };
        }
    }
}
