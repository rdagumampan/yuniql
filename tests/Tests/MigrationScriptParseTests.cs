using ArdiLabs.Yuniql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.IO;
using Shouldly;

namespace Yuniql.Tests
{
    [TestClass]
    public class MigrationScriptParseTests
    {
        [TestInitialize]
        public void Setup()
        {
            var workingPath = TestHelper.GetWorkingPath();
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }
        }

        [TestMethod]
        public void Test_Single_Run_Empty()
        {
            throw new System.NotImplementedException();
        }

        [TestMethod]
        public void Test_Single_Run_Single_Standard()
        {
            throw new System.NotImplementedException();
        }
        [TestMethod]
        public void Test_Run_Single_Without_GO()
        {
            throw new System.NotImplementedException();
        }
        [TestMethod]
        public void Test_Run_Multiple_Without_GO_In_Last_Line()
        {
            throw new System.NotImplementedException();
        }

        [TestMethod]
        public void Test_Run_Multiple_With_GO_In_The_Middle()
        {
            throw new System.NotImplementedException();
        }
        public void Test_Single_Run_Failed_Script_Must_Rollback()
        {
            throw new System.NotImplementedException();
        }
    }
}
