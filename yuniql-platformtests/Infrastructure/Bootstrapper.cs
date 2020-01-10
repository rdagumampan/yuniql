using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yuniql.PlatformTests
{
    [TestClass]
    public class Bootstrapper
    {

        [AssemblyInitialize]
        public static void SetupInfrastrucuture(TestContext testContext)
        {
            var containerFactory = new ContainerFactory();
            var container = containerFactory.Create("sqlserver");

            var dockerService = new DockerService();
            var foundContainer = dockerService.FindByName(container.Name).FirstOrDefault();
            if (null != foundContainer)
            {
                dockerService.Remove(foundContainer);
            }

            dockerService.Run(container);

            Thread.Sleep(1000 * 10);
        }

        [AssemblyCleanup]
        public static void TearDownInfrastructure()
        {
            var dockerService = new DockerService();
            var container = dockerService.FindByName("sqlserver-test-infra").First(); ;
            dockerService.Remove(container);
        }

        [TestMethod]
        public void BootstrapTest()
        {
            Assert.IsTrue("Just a placeholder test so boostrap is executed".Length > 0);
        }
    }
}
