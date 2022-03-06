using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Shouldly;
using Yuniql.Core;
using System;
using System.IO;
using System.Linq;
using Moq;
using Yuniql.Extensibility;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class DirectoryServiceTests: TestClassBase
    {
        [TestMethod]
        public void Test_Filter_Files_No_Environment_Aware_Directory_Present()
        {
            //arrange
            var traceService = new Mock<ITraceService>().Object;
            var basePath = Path.Combine(Environment.CurrentDirectory, "_db", RESERVED_DIRECTORY_NAME.INIT);
            string script1 = Path.Combine(basePath, "setup.sql");
            string script2 = Path.Combine(basePath, "tables", "setup.sql");
            string script3 = Path.Combine(basePath, "procedures", "setup.sql");
            string script4 = Path.Combine(basePath, "views", "setup.sql");
            var files = new List<string>
            {
                script1,
                script2,
                script3,
                script4,
            };

            //act
            var sut = new DirectoryService(traceService);
            var result = sut.FilterFiles(basePath, "test", files).ToList();

            //asset
            result.Count.ShouldBe(4);
            result[0].ShouldBe(script1);
            result[1].ShouldBe(script2);
            result[2].ShouldBe(script3);
            result[3].ShouldBe(script4);
        }

        [TestMethod]
        public void Test_Filter_Files_No_Environment_Code_Passed()
        {
            //arrange
            var traceService = new Mock<ITraceService>().Object;
            var basePath = Path.Combine(Environment.CurrentDirectory, "_db", RESERVED_DIRECTORY_NAME.INIT);
            string script1 = Path.Combine(basePath, "setup.sql");
            string script2 = Path.Combine(basePath, "tables", "setup.sql");
            string script3 = Path.Combine(basePath, "procedures", "setup.sql");
            string script4 = Path.Combine(basePath, "views", "setup.sql");
            var files = new List<string>
            {
                script1,
                script2,
                script3,
                script4,
            };

            //act
            var sut = new DirectoryService(traceService);
            var result = sut.FilterFiles(basePath, null, files).ToList();

            //asset
            result.Count.ShouldBe(4);
            result[0].ShouldBe(script1);
            result[1].ShouldBe(script2);
            result[2].ShouldBe(script3);
            result[3].ShouldBe(script4);
        }

        [TestMethod]
        public void Test_Filter_Files_Mixed_Files_And_Environment_Aware_Scripts()
        {
            //arrange
            var traceService = new Mock<ITraceService>().Object;
            var basePath = Path.Combine(Environment.CurrentDirectory, "_db", RESERVED_DIRECTORY_NAME.INIT);
            string script1 = Path.Combine(basePath, "setup.sql");
            string script2 = Path.Combine(basePath, "_dev", "setup.sql");
            string script3 = Path.Combine(basePath, "_test", "setup.sql");
            string script4 = Path.Combine(basePath, "_prod", "setup.sql");
            var files = new List<string>
            {
                script1,
                script2,
                script3,
                script4,
            };

            //act
            var sut = new DirectoryService(traceService);
            var result = sut.FilterFiles(basePath, "test", files).ToList();

            //asset
            result.Count.ShouldBe(2);
            result[0].ShouldBe(script1);
            result[1].ShouldBe(script3);
        }

        [TestMethod]
        public void Test_Filter_Files_Mixed_Files_And_Environment_Aware_Scripts_In_SubDirectory()
        {
            //arrange
            var traceService = new Mock<ITraceService>().Object;
            var basePath = Path.Combine(Environment.CurrentDirectory, "_db", RESERVED_DIRECTORY_NAME.INIT);
            string script1 = Path.Combine(basePath, "setup.sql");
            string script2 = Path.Combine(basePath, "tables", "_dev", "setup.sql");
            string script3 = Path.Combine(basePath, "tables", "_test", "setup.sql");
            string script4 = Path.Combine(basePath, "tables", "_prod", "setup.sql");
            var files = new List<string>
            {
                script1,
                script2,
                script3,
                script4,
            };

            //act
            var sut = new DirectoryService(traceService);
            var result = sut.FilterFiles(basePath, "test", files).ToList();

            //asset
            result.Count.ShouldBe(2);
            result[0].ShouldBe(script1);
            result[1].ShouldBe(script3);
        }

        [TestMethod]
        public void Test_Filter_Files_No_Environment_Code_Passed_But_Environment_Aware_Scripts_Is_Present()
        {
            //arrange
            var traceService = new Mock<ITraceService>().Object;
            var basePath = Path.Combine(Environment.CurrentDirectory, "_db", RESERVED_DIRECTORY_NAME.INIT);
            string script1 = Path.Combine(basePath, "setup.sql");
            string script2 = Path.Combine(basePath, "_dev", "setup.sql");
            string script3 = Path.Combine(basePath, "_test", "setup.sql");
            string script4 = Path.Combine(basePath, "_prod", "setup.sql");
            var files = new List<string>
            {
                script1,
                script2,
                script3,
                script4,
            };

            //act & asset
            Assert.ThrowsException<YuniqlMigrationException>(() =>
            {
                var sut = new DirectoryService(traceService);
                var result = sut.FilterFiles(basePath, null, files).ToList();
            }).Message.Contains("Found environment aware directories but no environment code passed.");
        }

        [TestMethod]
        public void Test_Filter_Files_Sub_Directories_Within_Environment_Aware_Directory()
        {
            //arrange
            var traceService = new Mock<ITraceService>().Object;
            var basePath = Path.Combine(Environment.CurrentDirectory, "_db", RESERVED_DIRECTORY_NAME.INIT);
            string script1 = Path.Combine(basePath, "setup.sql");
            string script2 = Path.Combine(basePath, "_dev", "tables", "setup.sql");
            string script3 = Path.Combine(basePath, "_test", "tables", "setup.sql");
            string script4 = Path.Combine(basePath, "_prod", "tables", "setup.sql");
            string script5 = Path.Combine(basePath, "_prod", "procedures", "setup.sql");
            string script6 = Path.Combine(basePath, "_prod", "views", "setup.sql");
            var files = new List<string>
            {
                script1,
                script2,
                script3,
                script4,
                script5,
                script6,
            };

            //act
            var sut = new DirectoryService(traceService);
            var result = sut.FilterFiles(basePath, "prod", files).ToList();

            //asset
            result.Count.ShouldBe(4);
            result[0].ShouldBe(script1);
            result[1].ShouldBe(script4);
            result[2].ShouldBe(script5);
            result[3].ShouldBe(script6);
        }

        [TestMethod]
        public void Test_Filter_Files_with_Script_FileNames_Have_Environment_Aware_Token_Underscore()
        {
            //arrange
            var traceService = new Mock<ITraceService>().Object;
            var basePath = Path.Combine(Environment.CurrentDirectory, "_db", RESERVED_DIRECTORY_NAME.INIT);
            string script1 = Path.Combine(basePath, "setup.sql");
            string script2 = Path.Combine(basePath, "tables", "_dev", "_setup.sql");
            string script3 = Path.Combine(basePath, "tables", "_test", "_setup_tables.sql");
            string script4 = Path.Combine(basePath, "tables", "_test", "_setup_stored_procedures.sql");
            string script5 = Path.Combine(basePath, "tables", "_prod", "_setup.sql");
            var files = new List<string>
            {
                script1,
                script2,
                script3,
                script4,
                script5
            };

            //act
            var sut = new DirectoryService(traceService);
            var result = sut.FilterFiles(basePath, "test", files).ToList();

            //asset
            result.Count.ShouldBe(3);
            result[0].ShouldBe(script1);
            result[1].ShouldBe(script3);
            result[2].ShouldBe(script4);
        }

        [TestMethod]
        public void Test_Filter_Files_No_Dir_With_Script_FileNames_Have_Environment_Aware_Token()
        {
            //arrange
            var traceService = new Mock<ITraceService>().Object;
            var basePath = Path.Combine(Environment.CurrentDirectory, "_db", RESERVED_DIRECTORY_NAME.INIT);
            string script1 = Path.Combine(basePath, "tables", "_setup_tables.sql");
            string script2 = Path.Combine(basePath, "tables", "_setup_stored_procedures.sql");
            string script3 = Path.Combine(basePath, "tables", "_dev", "_setup.sql");
            string script4 = Path.Combine(basePath, "tables", "_test", "_setup.sql");
            string script5 = Path.Combine(basePath, "tables", "_prod", "_setup.sql");
            var files = new List<string>
            {
                script1,
                script2,
                script3,
                script4,
                script5,
            };

            //act
            var sut = new DirectoryService(traceService);
            var result = sut.FilterFiles(basePath, "test", files).ToList();

            //asset
            result.Count.ShouldBe(3);
            result[0].ShouldBe(script1);
            result[1].ShouldBe(script2);
            result[2].ShouldBe(script4);
        }

        [TestMethod]
        public void Test_Filter_Files_No_Dir_With_Script_FileNames_Have_Environment_Aware_Token_No_Environment_Code_Passed()
        {
            //arrange
            var traceService = new Mock<ITraceService>().Object;
            var basePath = Path.Combine(Environment.CurrentDirectory, "_db", RESERVED_DIRECTORY_NAME.INIT);
            string script1 = Path.Combine(basePath, "tables", "_setup_tables.sql");
            string script2 = Path.Combine(basePath, "tables", "_setup_stored_procedures.sql");
            var files = new List<string>
            {
                script1,
                script2
            };

            //act
            var sut = new DirectoryService(traceService);
            var result = sut.FilterFiles(basePath, null, files).ToList();

            //asset
            result.Count.ShouldBe(2);
            result[0].ShouldBe(script1);
            result[1].ShouldBe(script2);
        }

        [TestMethod]
        public void Test_Filter_Files_Directories_With_Environment_Code_Passed()
        {
            //arrange
            var traceService = new Mock<ITraceService>().Object;
            var basePath = Path.Combine(Environment.CurrentDirectory, "_db", RESERVED_DIRECTORY_NAME.INIT);
            string script1 = Path.Combine(basePath, "setup_tables", "setup.sql");
            string script2 = Path.Combine(basePath, "setup_stored_procedures", "setup.sql");
            string script3 = Path.Combine(basePath, "setup_views", "setup.sql");
            var files = new List<string>
            {
                script1,
                script2,
                script3
            };

            //act
            var sut = new DirectoryService(traceService);
            var result = sut.FilterFiles(basePath, null, files).ToList();

            //asset
            result.Count.ShouldBe(3);
            result[0].ShouldBe(script1);
            result[1].ShouldBe(script2);
            result[2].ShouldBe(script3);
        }
    }
}
