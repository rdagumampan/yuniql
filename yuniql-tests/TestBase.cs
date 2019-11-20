using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;

namespace Yuniql.Tests
{
    [TestClass]
    public class TestBase
    {
        public string GetOrCreateWorkingPath()
        {
            var workingPath = Path.Combine(Environment.CurrentDirectory, @$"yuniql_testdb_{Guid.NewGuid().ToString().Substring(0, 4)}"); ;
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }

            return workingPath;
        }

        public void CleanUp(string workingPath)
        {
            if (Directory.Exists(workingPath))
            {
                Directory.Delete(workingPath, true);
            }
        }
    }
}
