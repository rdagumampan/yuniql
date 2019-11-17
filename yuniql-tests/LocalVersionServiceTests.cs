using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.SqlServer.Tests
{

    [TestClass]
    public class LocalVersionServiceTests : TestBase
    {
        private string _targetPlatform;
        private ITestDataService _testDataService;

        private IMigrationServiceFactory _migrationServiceFactory;
        private ITraceService _traceService;

        [TestInitialize]
        public void Setup()
        {
            //get target platform to tests from environment variable
            _targetPlatform = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_TARGET_PLATFORM");
            if (string.IsNullOrEmpty(_targetPlatform))
            {
                _targetPlatform = "sqlserver";
            }

            //create test data service provider
            var testDataServiceFactory = new TestDataServiceFactory();
            _testDataService = testDataServiceFactory.Create(_targetPlatform);

            //create data service factory for migration proper
            _traceService = new TraceService();
            _migrationServiceFactory = new MigrationServiceFactory(_traceService);
        }

        [TestMethod]
        public void Test_Init()
        {
            //act
            var workingPath = GetOrCreateWorkingPath();

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            //assert
            Directory.Exists(Path.Combine(workingPath, "_init")).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(workingPath, "_init"), "README.md")).ShouldBe(true);

            Directory.Exists(Path.Combine(workingPath, "_pre")).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(workingPath, "_pre"), "README.md")).ShouldBe(true);

            Directory.Exists(Path.Combine(workingPath, "v0.00")).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(workingPath, "v0.00"), "README.md")).ShouldBe(true);

            Directory.Exists(Path.Combine(workingPath, "_draft")).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(workingPath, "_draft"), "README.md")).ShouldBe(true);

            Directory.Exists(Path.Combine(workingPath, "_post")).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(workingPath, "_post"), "README.md")).ShouldBe(true);

            Directory.Exists(Path.Combine(workingPath, "_erase")).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(workingPath, "_erase"), "README.md")).ShouldBe(true);

            File.Exists(Path.Combine(workingPath, "README.md")).ShouldBe(true);
            File.Exists(Path.Combine(workingPath, "Dockerfile")).ShouldBe(true);
            File.Exists(Path.Combine(workingPath, ".gitignore")).ShouldBe(true);
        }

        [TestMethod]
        public void Test_Init_Called_Multiple_Is_Handled()
        {
            //act
            var workingPath = GetOrCreateWorkingPath();

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.Init(workingPath);
            localVersionService.Init(workingPath);

            //assert
            Directory.Exists(Path.Combine(workingPath, "_init")).ShouldBe(true);
            Directory.Exists(Path.Combine(workingPath, "_pre")).ShouldBe(true);
            Directory.Exists(Path.Combine(workingPath, "v0.00")).ShouldBe(true);
            Directory.Exists(Path.Combine(workingPath, "_draft")).ShouldBe(true);
            Directory.Exists(Path.Combine(workingPath, "_post")).ShouldBe(true);
            File.Exists(Path.Combine(workingPath, "README.md")).ShouldBe(true);
            File.Exists(Path.Combine(workingPath, "Dockerfile")).ShouldBe(true);
            File.Exists(Path.Combine(workingPath, ".gitignore")).ShouldBe(true);
        }

        [TestMethod]
        public void Test_Increment_Major_Version()
        {
            //act
            var workingPath = GetOrCreateWorkingPath();

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);

            //assert
            Directory.Exists(Path.Combine(workingPath, "v1.00")).ShouldBe(true);
        }

        [TestMethod]
        public void Test_Increment_Major_Version_With_Template_File()
        {
            //act
            var workingPath = GetOrCreateWorkingPath();

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, "Test.sql");

            //assert
            Directory.Exists(Path.Combine(workingPath, "v1.00")).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(workingPath, "v1.00"), "Test.sql")).ShouldBe(true);
        }

        [TestMethod]
        public void Test_Increment_Minor_Version()
        {
            //act
            var workingPath = GetOrCreateWorkingPath();

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMinorVersion(workingPath, null);

            //assert
            Directory.Exists(Path.Combine(workingPath, "v0.01")).ShouldBe(true);
        }

        [TestMethod]
        public void Test_Increment_Minor_Version_With_Template_File()
        {
            //act
            var workingPath = GetOrCreateWorkingPath();

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMinorVersion(workingPath, "Test.sql");

            //assert
            Directory.Exists(Path.Combine(workingPath, "v0.01")).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(workingPath, "v0.01"), "Test.sql")).ShouldBe(true);
        }

        [TestMethod]
        public void Test_Get_Latest_Version()
        {
            //act
            var workingPath = GetOrCreateWorkingPath();

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);
            localVersionService.IncrementMinorVersion(workingPath, null);
            localVersionService.IncrementMinorVersion(workingPath, null);

            //assert
            localVersionService.GetLatestVersion(workingPath).ShouldBe("v1.02");
        }
    }
}
