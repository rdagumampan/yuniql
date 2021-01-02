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

        public Configuration GetFreshConfiguration()
        {
            var traceService = new FileTraceService();
            var environmentService = new EnvironmentService();
            var localVersionService = new LocalVersionService(traceService);
            var configurationService = new ConfigurationService(environmentService, localVersionService, traceService);
            configurationService.Reset();

            var configuration = Configuration.Instance;
            configuration.Workspace = this.WorkspacePath;
            configuration.Platform = this.Platform;
            configuration.ConnectionString = this.ConnectionString;
            configuration.IsAutoCreateDatabase = true;

            return configuration;
        }
    }
}
