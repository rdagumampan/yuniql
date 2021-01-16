using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.PlatformTests.Infrastructure
{
    [TestClass]
    public class StartupInfrastructure
    {
        [AssemblyInitialize]
        public static void SetupInfrastrucuture(TestContext testContext)
        {
            var testAgentHost = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_HOST);
            var targetPlatform = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_PLATFORM);

            if (string.IsNullOrEmpty(testAgentHost) || string.IsNullOrWhiteSpace(testAgentHost) || testAgentHost.ToUpper().Equals("CONTAINER"))
            {
                var containerFactory = new ContainerFactory();
                var container = containerFactory.Create(targetPlatform.ToLower());
                var image = new DockerImage
                {
                    Image = container.Image,
                    Tag = container.Tag
                };

                var dockerService = new DockerService();
                var foundContainer = dockerService.FindByName(container.Name).FirstOrDefault();
                if (null != foundContainer)
                {
                    dockerService.Remove(foundContainer);
                }

                dockerService.Pull(image);
                dockerService.Run(container);

                //TODO: implement connection ping with timeout
                Thread.Sleep(1000 * 30);
            }
        }

        [AssemblyCleanup]
        public static void TearDownInfrastructure()
        {
            var testAgentHost = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_HOST);
            var targetPlatform = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_PLATFORM);

            if (string.IsNullOrEmpty(testAgentHost) || string.IsNullOrWhiteSpace(testAgentHost) || testAgentHost.ToUpper().Equals("CONTAINER"))
            {
                var dockerService = new DockerService();
                var container = dockerService.FindByName($"{targetPlatform.ToLower()}-test-infra").First(); ;
                dockerService.Remove(container);
            }
        }

        [TestMethod]
        public void BootstrapTest()
        {
            Assert.IsTrue("Just a placeholder test so boostrap is executed. Do not remove this.".Length > 0);
        }
    }
}
