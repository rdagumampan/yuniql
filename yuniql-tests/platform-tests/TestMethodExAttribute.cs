using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.PlatformTests
{
    public class TestMethodExAttribute : TestMethodAttribute
    {
        public string Requires { get; set; }

        public override TestResult[] Execute(ITestMethod testMethod)
        {
            var platform = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_TARGET_PLATFORM);
            var testDataServiceFactory = new TestDataServiceFactory();
            var testDataService = testDataServiceFactory.Create(platform);

            //Ignores test methods with [TestMethodExAttribute (Requires = "IsAtomicDDLSupported")] attribute
            if (this.Requires.Contains(nameof(testDataService.IsAtomicDDLSupported)) && !testDataService.IsAtomicDDLSupported)
            {
                var message = $"Target database platform or version does not support atomic DDL operations. " +
                    $"DDL operations like CREATE TABLE, CREATE VIEW are not gauranteed to be executed transactional.";
                return new[]
                {
                    new TestResult
                    {
                        Outcome = UnitTestOutcome.NotRunnable,
                        LogOutput = message
                    }
                };
            }


            //Ignores test methods with [TestMethodExAttribute (Requires = "IsAtomicDDLSupported")] attribute
            if (this.Requires.Contains("IsAtomicDDLNotSupported") && testDataService.IsAtomicDDLSupported)
            {
                var message = $"Target database platform or version supports atomic DDL operations. " +
                    $"This test is exclusive for platforms not supporting transaction DDL operations.";
                return new[]
                {
                    new TestResult
                    {
                        Outcome = UnitTestOutcome.NotRunnable,
                        LogOutput = message
                    }
                };
            }

            //Ignores test methods with [TestMethodExAttribute (Requires = "IsSchemaSupported")] attribute
            if (this.Requires.Contains(nameof(testDataService.IsSchemaSupported)) && !testDataService.IsSchemaSupported)
            {
                var message = $"Target database platform or version does not support schema within the same database.";
                return new[]
                {
                    new TestResult
                    {
                        Outcome = UnitTestOutcome.NotRunnable,
                        LogOutput = message
                    }
                };
            }

            //Ignores test methods with [TestMethodExAttribute (Requires = "IsBatchSqlSupported")] attribute
            if (this.Requires.Contains(nameof(testDataService.IsBatchSqlSupported)) && !testDataService.IsBatchSqlSupported)
            {
                var message = $"Target database platform or version does not support schema within the same database.";
                return new[]
                {
                    new TestResult
                    {
                        Outcome = UnitTestOutcome.NotRunnable,
                        LogOutput = message
                    }
                };
            }

            return base.Execute(testMethod);
        }
    }
}
