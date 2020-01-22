using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yuniql.PlatformTests
{
    public class TestMethodExAttribute : TestMethodAttribute
    {
        public string Filter { get; set; }

        public override TestResult[] Execute(ITestMethod testMethod)
        {
            var platform = EnvironmentHelper.GetEnvironmentVariable(EnvironmentVariableNames.YUNIQL_TEST_TARGET_PLATFORM);
            var testDataServiceFactory = new TestDataServiceFactory();
            var testDataService = testDataServiceFactory.Create(platform);

            if (this.Filter.Contains(nameof(testDataService.IsAtomicDDLSupported)) && !testDataService.IsAtomicDDLSupported)
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

            if (this.Filter.Contains(nameof(testDataService.IsSchemaSupported)) && !testDataService.IsSchemaSupported)
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
