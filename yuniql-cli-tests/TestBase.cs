using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CliTests
{
    [TestClass]
    public class TestBase
    {
        public string CreateEmptyWorkspace()
        {
            //prepare test directory
            var workingPath = Path.Combine(Environment.CurrentDirectory, @$"yuniql_cli_test_{Guid.NewGuid().ToString().Substring(0, 6)}"); ;
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }
            return workingPath;
        }

        public string CloneSampleWorkspace(string workingPath)
        {
            //copy sample project from root
            var source = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_SAMPLEDB");
            var destination = workingPath;

            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(source, destination));
            }

            foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(source, destination), true);
            }

            return workingPath;
        }
    }
}
