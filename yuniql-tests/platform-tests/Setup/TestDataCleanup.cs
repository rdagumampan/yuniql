using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Yuniql.Core;
using Yuniql.Extensibility;
using Yuniql.PlatformTests.Interfaces;
using IMigrationServiceFactory = Yuniql.PlatformTests.Interfaces.IMigrationServiceFactory;
using MigrationServiceFactory = Yuniql.PlatformTests.Setup.MigrationServiceFactory;

namespace Yuniql.PlatformTests.Setup
{
    [TestClass]
    public class TestDataCleanup: TestClassBase
    {
        private ITestDataService _testDataService;
        private ITraceService _traceService;
        private IDirectoryService _directoryService;
        private IMigrationServiceFactory _migrationServiceFactory;
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
            catch (Exception) { /*swallow exceptions*/ }
        }

        [TestMethodEx(Requires = "IsMultiTenancySupported")]
        public void Test_Drop_All_Test_Databases()
        {
            var databases = new List<string> {

};

            //get default connection string template from env variable
            var connectionString = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_CONNECTION_STRING);
            if (!connectionString.Contains("=yuniqldb"))
            {
                throw new Exception("Your default database in your test connection string must be \"yuniqldb\". This is replaced during test execution with test database per test case.");
            }

            databases.ForEach(database => {
                try
                {
                    var targetConnectionString = connectionString.Replace("yuniqldb", database);
                    _testDataService.CleanupDbObjects(targetConnectionString);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            });
        }
    }
}
