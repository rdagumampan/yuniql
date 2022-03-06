using Yuniql.Core;

namespace Yuniql.PlatformTests.Setup
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
            var directoryService = new DirectoryService(traceService);
            var environmentService = new EnvironmentService();
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(traceService, directoryService, fileService);
            var configurationService = new ConfigurationService(environmentService, workspaceService, traceService);
            configurationService.Reset();

            var configuration = Configuration.Instance;
            configuration.Workspace = WorkspacePath;
            configuration.Platform = Platform;
            configuration.ConnectionString = ConnectionString;
            configuration.IsAutoCreateDatabase = true;

            return configuration;
        }
    }
}
