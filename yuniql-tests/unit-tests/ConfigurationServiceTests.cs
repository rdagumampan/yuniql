using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class ConfigurationServiceTests
    {
        [TestMethod]
        public void Test_GetValueOrDefault_With_Use_Original_Value()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var localVersionService = new Mock<ILocalVersionService>();

            //act
            var sut = new ConfigurationService(environmentService.Object, localVersionService.Object, traceService.Object);
            var result = sut.GetValueOrDefault("mariadb", ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER);

            //assert
            result.ShouldBe("mariadb");
        }

        [TestMethod]
        public void Test_GetValueOrDefault_Use_Environment_Variable()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM)).Returns(SUPPORTED_DATABASES.MARIADB);
            var localVersionService = new Mock<ILocalVersionService>();

            //act
            var sut = new ConfigurationService(environmentService.Object, localVersionService.Object, traceService.Object);
            var result = sut.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER);

            //assert
            result.ShouldBe(SUPPORTED_DATABASES.MARIADB);
        }

        [TestMethod]
        public void Test_GetValueOrDefault_Use_Default_Value()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var localVersionService = new Mock<ILocalVersionService>();

            //act
            var sut = new ConfigurationService(environmentService.Object, localVersionService.Object, traceService.Object);
            var result = sut.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER);

            //assert
            result.ShouldBe(SUPPORTED_DATABASES.SQLSERVER);
        }

    }

}
