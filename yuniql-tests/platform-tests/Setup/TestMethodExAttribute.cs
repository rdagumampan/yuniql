using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.PlatformTests.Setup
{
    public class TestMethodExAttribute : TestMethodAttribute
    {
        public string Requires { get; set; }

        public override TestResult[] Execute(ITestMethod testMethod)
        {
            var platform = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_PLATFORM);
            var testDataServiceFactory = new TestDataServiceFactory();
            var testDataService = testDataServiceFactory.Create(platform);

            //Ignores test methods with [TestMethodExAttribute (Requires = "IsTransactionalDdlNotSupported")] attribute
            //For test cases applicable only to platforms that do not support for transactional DDL (mysql, snowflake, ...)
            if (Requires.Contains("IsMultiTenancySupported") && !testDataService.IsMultiTenancySupported)
            {
                var message = $"Target database platform or version does not support multitenancy. " +
                    $"The platform only supports single database for the given server instance.";
                return new[]
                {
                    new TestResult
                    {
                        Outcome = UnitTestOutcome.Inconclusive,
                        LogOutput = message
                    }
                };
            }

            //Ignores test methods with [TestMethodExAttribute (Requires = "IsTransactionalDdlSupported")] attribute
            //For test cases applicable only to platforms that has full support for transactional DDL (sqlserver, pgsql, ...)
            if (Requires.Contains(nameof(testDataService.IsTransactionalDdlSupported)) && !testDataService.IsTransactionalDdlSupported)
            {
                var message = $"Target database platform or version does not support atomic DDL operations. " +
                    $"DDL operations like CREATE TABLE, CREATE VIEW are not gauranteed to be executed transactional.";
                return new[]
                {
                    new TestResult
                    {
                        Outcome = UnitTestOutcome.Inconclusive,
                        LogOutput = message
                    }
                };
            }

            //Ignores test methods with [TestMethodExAttribute (Requires = "IsTransactionalDdlNotSupported")] attribute
            //For test cases applicable only to platforms that do not support for transactional DDL (mysql, snowflake, ...)
            if (Requires.Contains("IsTransactionalDdlNotSupported") && testDataService.IsTransactionalDdlSupported)
            {
                var message = $"Target database platform or version does not support atomic DDL operations. " +
                    $"DDL operations like CREATE TABLE, CREATE VIEW are not gauranteed to be executed transactional.";
                return new[]
                {
                    new TestResult
                    {
                        Outcome = UnitTestOutcome.Inconclusive,
                        LogOutput = message
                    }
                };
            }

            //Ignores test methods with [TestMethodExAttribute (Requires = "IsSchemaSupported")] attribute
            //For test cases applicable only to platforms that do not support schema
            if (Requires.Contains(nameof(testDataService.IsSchemaSupported)) && !testDataService.IsSchemaSupported)
            {
                var message = $"Target database platform or version does not support schema within the same database.";
                return new[]
                {
                    new TestResult
                    {
                        Outcome = UnitTestOutcome.Inconclusive,
                        LogOutput = message
                    }
                };
            }

            //Ignores test methods with [TestMethodExAttribute (Requires = "IsBatchSqlSupported")] attribute
            if (Requires.Contains(nameof(testDataService.IsBatchSqlSupported)) && !testDataService.IsBatchSqlSupported)
            {
                var message = $"Target database platform or version does not support batching sql statements in single session or request.";
                return new[]
                {
                    new TestResult
                    {
                        Outcome = UnitTestOutcome.Inconclusive,
                        LogOutput = message
                    }
                };
            }

            //Ignores test methods with [TestMethodExAttribute (Requires = "IsBatchSqlSupportedAndIsTransactionalDdlSupported")] attribute
            if (Requires.Contains(nameof(TestDataServiceBase.IsBatchSqlSupported) + "And" + nameof(TestDataServiceBase.IsTransactionalDdlSupported)) 
                && (!testDataService.IsBatchSqlSupported || !testDataService.IsTransactionalDdlSupported))
            {
                var message = $"Target database platform or version does not support batching sql statements in single session or request.";
                return new[]
                {
                    new TestResult
                    {
                        Outcome = UnitTestOutcome.Inconclusive,
                        LogOutput = message
                    }
                };
            }

            return base.Execute(testMethod);
        }
    }
}
