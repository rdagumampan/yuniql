using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using Yuniql.Core;
using Yuniql.Extensibility;
using System;
using Yuniql.PlatformTests.Interfaces;
using Yuniql.PlatformTests.Setup;
using IMigrationServiceFactory = Yuniql.PlatformTests.Interfaces.IMigrationServiceFactory;
using MigrationServiceFactory = Yuniql.PlatformTests.Setup.MigrationServiceFactory;
using System.Diagnostics;

namespace Yuniql.PlatformTests.Core
{

    [TestClass]
    public class WorkspaceServiceTests : TestClassBase
    {
        private ITestDataService _testDataService;
        private IMigrationServiceFactory _migrationServiceFactory;
        private ITraceService _traceService;
        private IDirectoryService _directoryService;
        private TestConfiguration _testConfiguration;
        [TestInitialize]
        public void Setup()
        {
            _testConfiguration = ConfigureWithEmptyWorkspace();

            //create test data service provider
            var testDataServiceFactory = new TestDataServiceFactory();
            _testDataService = testDataServiceFactory.Create(_testConfiguration.Platform);

            //create data service factory for migration proper
            _directoryService = new DirectoryService(_traceService);
            _traceService = new FileTraceService() { IsDebugEnabled = true };
            _migrationServiceFactory = new MigrationServiceFactory(_traceService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            //drop the test directory
            try
            {
                if (Directory.Exists(_testConfiguration.WorkspacePath))
                    Directory.Delete(_testConfiguration.WorkspacePath, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            //drop test database
            try
            {
                _testDataService.CleanupDbObjects(_testConfiguration.ConnectionString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        [TestMethod]
        public void Test_Init()
        {
            //act
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //assert
            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.INIT)).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.INIT), "README.md")).ShouldBe(true);

            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.PRE)).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.PRE), "README.md")).ShouldBe(true);

            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, "v0.00")).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), "README.md")).ShouldBe(true);

            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.DRAFT)).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.DRAFT), "README.md")).ShouldBe(true);

            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.POST)).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.POST), "README.md")).ShouldBe(true);

            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.ERASE)).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.ERASE), "README.md")).ShouldBe(true);

            File.Exists(Path.Combine(_testConfiguration.WorkspacePath, "README.md")).ShouldBe(true);
            File.Exists(Path.Combine(_testConfiguration.WorkspacePath, "Dockerfile")).ShouldBe(true);
            File.Exists(Path.Combine(_testConfiguration.WorkspacePath, ".gitignore")).ShouldBe(true);
        }

        [TestMethod]
        public void Test_Init_Called_Multiple_Times_Is_Handled()
        {
            //act
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //assert
            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.INIT)).ShouldBe(true);
            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.PRE)).ShouldBe(true);
            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, "v0.00")).ShouldBe(true);
            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.DRAFT)).ShouldBe(true);
            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.POST)).ShouldBe(true);
            File.Exists(Path.Combine(_testConfiguration.WorkspacePath, "README.md")).ShouldBe(true);
            File.Exists(Path.Combine(_testConfiguration.WorkspacePath, "Dockerfile")).ShouldBe(true);
            File.Exists(Path.Combine(_testConfiguration.WorkspacePath, ".gitignore")).ShouldBe(true);
        }

        [TestMethod]
        public void Test_Vnext_Major_Version()
        {
            //act
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            //assert
            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, "v1.00")).ShouldBe(true);
        }

        [TestMethod]
        public void Test_Vnext_Major_Version_With_Template_File()
        {
            //act
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, "Test.sql");

            //assert
            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, "v1.00")).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), "Test.sql")).ShouldBe(true);
        }

        [TestMethod]
        public void Test_Vnext_Minor_Version()
        {
            //act
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);

            //assert
            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, "v0.01")).ShouldBe(true);
        }

        [TestMethod]
        public void Test_Vnext_Minor_Version_With_Template_File()
        {
            //act
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, "Test.sql");

            //assert
            Directory.Exists(Path.Combine(_testConfiguration.WorkspacePath, "v0.01")).ShouldBe(true);
            File.Exists(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.01"), "Test.sql")).ShouldBe(true);
        }

        [TestMethod]
        public void Test_Get_Latest_Version()
        {
            //act
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);

            //assert
            workspaceService.GetLatestVersion(_testConfiguration.WorkspacePath).ShouldBe("v1.02");
        }
    }
}
