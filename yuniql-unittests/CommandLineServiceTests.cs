using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
using Yuniql.CLI;
using Moq;
using Yuniql.Extensibility;
using Yuniql.Core;

namespace Yuniql.Tests
{
    [TestClass]
    public class CommandLineServiceTests
    {
        [TestMethod]
        public void Test_Init_Option_No_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var localVersionService = new Mock<ILocalVersionService>();

            //act
            var option = new InitOption();
            var sut = new CommandLineService(localVersionService.Object, environmentService.Object, traceService.Object);
            sut.RunInitOption(option);

            //assert
            localVersionService.Verify(s => s.Init(@"c:\temp\yuniql"));
        }

        [TestMethod]
        public void Test_Init_Option_With_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var localVersionService = new Mock<ILocalVersionService>();

            //act
            var option = new InitOption { Path = @"c:\temp\yuniql-ex" };
            var sut = new CommandLineService(localVersionService.Object, environmentService.Object, traceService.Object);
            sut.RunInitOption(option);

            //assert
            localVersionService.Verify(s => s.Init(@"c:\temp\yuniql-ex"));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_Major_No_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var localVersionService = new Mock<ILocalVersionService>();

            //act
            var option = new NextVersionOption { IncrementMajorVersion = true };
            var sut = new CommandLineService(localVersionService.Object, environmentService.Object, traceService.Object);
            sut.IncrementVersion(option);

            //assert
            localVersionService.Verify(s => s.IncrementMajorVersion(@"c:\temp\yuniql", null));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_Major_With_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var localVersionService = new Mock<ILocalVersionService>();

            //act
            var option = new NextVersionOption { IncrementMajorVersion = true, Path = @"c:\temp\yuniql-ex" };
            var sut = new CommandLineService(localVersionService.Object, environmentService.Object, traceService.Object);
            sut.IncrementVersion(option);

            //assert
            localVersionService.Verify(s => s.IncrementMajorVersion(@"c:\temp\yuniql-ex", null));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_Minor_No_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var localVersionService = new Mock<ILocalVersionService>();

            //act
            var option = new NextVersionOption { IncrementMinorVersion = true };
            var sut = new CommandLineService(localVersionService.Object, environmentService.Object, traceService.Object);
            sut.IncrementVersion(option);

            //assert
            localVersionService.Verify(s => s.IncrementMinorVersion(@"c:\temp\yuniql", null));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_No_Explicit_Options()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var localVersionService = new Mock<ILocalVersionService>();

            //act
            var option = new NextVersionOption { };
            var sut = new CommandLineService(localVersionService.Object, environmentService.Object, traceService.Object);
            sut.IncrementVersion(option);

            //assert
            localVersionService.Verify(s => s.IncrementMinorVersion(@"c:\temp\yuniql", null));
        }
    }
}
