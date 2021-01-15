using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Yuniql.Core;
using Yuniql.Extensibility;
using Shouldly;
using System.IO;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class WorkspaceServiceTests : TestClassBase
    {
        [TestMethod]
        public void Test_Init()
        {
            //arrange
            var workspace = @"c:\temp\yuniql";
            var traceService = new Mock<ITraceService>();
            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_FILE_NAME.README))).Returns(false);
            fileService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_FILE_NAME.DOCKER_FILE))).Returns(false);
            fileService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_FILE_NAME.GIT_IGNORE_FILE))).Returns(false);

            var directoryService = new Mock<IDirectoryService>();
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT))).Returns(false);
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE))).Returns(false);
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT))).Returns(false);
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST))).Returns(false);
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.ERASE))).Returns(false);
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.BASELINE))).Returns(false);

            //act
            var sut = new WorkspaceService(traceService.Object, directoryService.Object, fileService.Object);
            sut.Init(workspace);

            //assert
            directoryService.Verify(s => s.CreateDirectory(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT)));
            directoryService.Verify(s => s.CreateDirectory(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE)));
            directoryService.Verify(s => s.CreateDirectory(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT)));
            directoryService.Verify(s => s.CreateDirectory(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT)));
            directoryService.Verify(s => s.CreateDirectory(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST)));
            directoryService.Verify(s => s.CreateDirectory(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.ERASE)));
            directoryService.Verify(s => s.CreateDirectory(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.BASELINE)));

            fileService.Verify(s => s.AppendAllText(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT, RESERVED_FILE_NAME.README), It.IsAny<string>()));
            fileService.Verify(s => s.AppendAllText(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE, RESERVED_FILE_NAME.README), It.IsAny<string>()));
            fileService.Verify(s => s.AppendAllText(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT, RESERVED_FILE_NAME.README), It.IsAny<string>()));
            fileService.Verify(s => s.AppendAllText(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST, RESERVED_FILE_NAME.README), It.IsAny<string>()));
            fileService.Verify(s => s.AppendAllText(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.ERASE, RESERVED_FILE_NAME.README), It.IsAny<string>()));
            fileService.Verify(s => s.AppendAllText(Path.Combine(workspace, RESERVED_FILE_NAME.README), It.IsAny<string>()));
            fileService.Verify(s => s.AppendAllText(Path.Combine(workspace, RESERVED_FILE_NAME.DOCKER_FILE), It.IsAny<string>()));
            fileService.Verify(s => s.AppendAllText(Path.Combine(workspace, RESERVED_FILE_NAME.GIT_IGNORE_FILE), It.IsAny<string>()));

            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT)));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE)));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT)));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST)));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.ERASE)));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.BASELINE)));

            fileService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_FILE_NAME.README)));
            fileService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_FILE_NAME.DOCKER_FILE)));
            fileService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_FILE_NAME.GIT_IGNORE_FILE)));
        }


        [TestMethod]
        public void Test_Get_Latest_Version()
        {
            //arrange
            var workspace = @"c:\temp\yuniql";
            var traceService = new Mock<ITraceService>();
            var fileService = new Mock<IFileService>();

            var directoryService = new Mock<IDirectoryService>();
            directoryService.Setup(s => s.GetDirectories(workspace, "v*.*")).Returns(new string[] {
                @$"{workspace}\v0.00",
                @$"{workspace}\v0.02",  //simulate an out of order list
                @$"{workspace}\v0.01",
                }
            );

            //act
            var sut = new WorkspaceService(traceService.Object, directoryService.Object, fileService.Object);
            var latestVersion = sut.GetLatestVersion(workspace);

            //assert
            directoryService.Verify(s => s.GetDirectories(workspace, "v*.*"));
            latestVersion.ShouldBe("v0.02");
        }

        [TestMethod]
        public void Test_Get_Baseline_As_Latest_Version()
        {
            //arrange
            var workspace = @"c:\temp\yuniql";
            var traceService = new Mock<ITraceService>();
            var fileService = new Mock<IFileService>();

            var directoryService = new Mock<IDirectoryService>();
            directoryService.Setup(s => s.GetDirectories(workspace, "v*.*")).Returns(new string[] { @$"{workspace}\v0.00" }
            );

            //act
            var sut = new WorkspaceService(traceService.Object, directoryService.Object, fileService.Object);
            var latestVersion = sut.GetLatestVersion(workspace);

            //assert
            directoryService.Verify(s => s.GetDirectories(workspace, "v*.*"));
            latestVersion.ShouldBe("v0.00");
        }

        [TestMethod()]
        public void Test_Increment_Major_Version()
        {
            //arrange
            var workspace = @"c:\temp\yuniql";
            var traceService = new Mock<ITraceService>();
            var fileService = new Mock<IFileService>();

            var directoryService = new Mock<IDirectoryService>();
            directoryService.Setup(s => s.GetDirectories(workspace, "v*.*")).Returns(new string[] {
                @$"{workspace}\v0.00",
                @$"{workspace}\v0.02",  //simulate an out of order list
                @$"{workspace}\v0.01",
                }
            );
            //act
            var sut = new WorkspaceService(traceService.Object, directoryService.Object, fileService.Object);
            var result = sut.IncrementMajorVersion(workspace, sqlFileName: null);

            //assert
            result.ShouldBe("v1.00");
            directoryService.Verify(s => s.GetDirectories(workspace, "v*.*"));
            directoryService.Verify(s => s.CreateDirectory(@$"{workspace}\v1.00"));
        }


        [TestMethod()]
        public void Test_Increment_Major_Version_With_Sql_File()
        {
            //arrange
            var workspace = @"c:\temp\yuniql";
            var traceService = new Mock<ITraceService>();
            var fileService = new Mock<IFileService>();

            var directoryService = new Mock<IDirectoryService>();
            directoryService.Setup(s => s.GetDirectories(workspace, "v*.*")).Returns(new string[] {
                @$"{workspace}\v0.00",
                @$"{workspace}\v0.02",  //simulate an out of order list
                @$"{workspace}\v0.01",
                }
            );
            //act
            var sut = new WorkspaceService(traceService.Object, directoryService.Object, fileService.Object);
            var result = sut.IncrementMajorVersion(workspace, sqlFileName: "testscript.sql");

            //assert
            result.ShouldBe("v1.00");
            directoryService.Verify(s => s.GetDirectories(workspace, "v*.*"));
            directoryService.Verify(s => s.CreateDirectory(@$"{workspace}\v1.00"));
            fileService.Verify(s => s.AppendAllText(@$"{workspace}\v1.00\testscript.sql", @""));
        }

        [TestMethod]
        public void Test_Increment_Minor_Version()
        {
            //arrange
            var workspace = @"c:\temp\yuniql";
            var traceService = new Mock<ITraceService>();
            var fileService = new Mock<IFileService>();

            var directoryService = new Mock<IDirectoryService>();
            directoryService.Setup(s => s.GetDirectories(workspace, "v*.*")).Returns(new string[] {
                @$"{workspace}\v0.00",
                @$"{workspace}\v0.02",  //simulate an out of order list
                @$"{workspace}\v0.01",
                }
            );
            //act
            var sut = new WorkspaceService(traceService.Object, directoryService.Object, fileService.Object);
            var result = sut.IncrementMinorVersion(workspace, sqlFileName: null);

            //assert
            result.ShouldBe("v0.03");
            directoryService.Verify(s => s.GetDirectories(workspace, "v*.*"));
            directoryService.Verify(s => s.CreateDirectory(@$"{workspace}\v0.03"));
        }

        [TestMethod]
        public void Test_Increment_Minor_Version_With_Sql_File()
        {
            //arrange
            var workspace = @"c:\temp\yuniql";
            var traceService = new Mock<ITraceService>();
            var fileService = new Mock<IFileService>();

            var directoryService = new Mock<IDirectoryService>();
            directoryService.Setup(s => s.GetDirectories(workspace, "v*.*")).Returns(new string[] {
                @$"{workspace}\v0.00",
                @$"{workspace}\v0.02",  //simulate an out of order list
                @$"{workspace}\v0.01",
                }
            );
            //act
            var sut = new WorkspaceService(traceService.Object, directoryService.Object, fileService.Object);
            var result = sut.IncrementMinorVersion(workspace, sqlFileName: "testscript.sql");

            //assert
            result.ShouldBe("v0.03");
            directoryService.Verify(s => s.GetDirectories(workspace, "v*.*"));
            directoryService.Verify(s => s.CreateDirectory(@$"{workspace}\v0.03"));
            fileService.Verify(s => s.AppendAllText(@$"{workspace}\v0.03\testscript.sql", @""));
        }

        [TestMethod]
        public void Test_Validate_Good_Setup()
        {
            //arrange
            var workspace = @"c:\temp\yuniql";
            var traceService = new Mock<ITraceService>();
            var fileService = new Mock<IFileService>();

            var directoryService = new Mock<IDirectoryService>();
            directoryService.Setup(s => s.GetDirectories(workspace, "v0.00*")).Returns(new string[] { @$"{workspace}\v0.00" });
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT))).Returns(true);
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE))).Returns(true);
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT))).Returns(true);
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST))).Returns(true);
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.ERASE))).Returns(true);

            //act
            var sut = new WorkspaceService(traceService.Object, directoryService.Object, fileService.Object);
            sut.Validate(workspace);

            //assert
            directoryService.Verify(s => s.GetDirectories(workspace, "v0.00*"));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT)));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE)));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT)));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST)));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.ERASE)));
        }

        [TestMethod]
        public void Test_Validate_Missing_Directories()
        {
            //arrange
            var workspace = @"c:\temp\yuniql";
            var traceService = new Mock<ITraceService>();
            var fileService = new Mock<IFileService>();

            var directoryService = new Mock<IDirectoryService>();
            directoryService.Setup(s => s.GetDirectories(workspace, "v0.00*")).Returns(new string[] { @$"{workspace}\v0.00" });
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT))).Returns(false);
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE))).Returns(false);
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT))).Returns(false);
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST))).Returns(false);
            directoryService.Setup(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.ERASE))).Returns(false);

            //act
            var exception = Assert.ThrowsException<YuniqlMigrationException>(() =>
            {
                var sut = new WorkspaceService(traceService.Object, directoryService.Object, fileService.Object);
                sut.Validate(workspace);
            });

            //assert
            exception.Message.Contains($"{Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT)} / Missing!").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE)} / Missing!").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT)} / Missing!").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST)} / Missing!").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(workspace, RESERVED_DIRECTORY_NAME.ERASE)} / Missing!").ShouldBeTrue();

            directoryService.Verify(s => s.GetDirectories(workspace, "v0.00*"));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT)));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE)));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT)));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST)));
            directoryService.Verify(s => s.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.ERASE)));
        }
    }
}
