using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.UnitTests
{
    //TODO: Implement LocalVersionServiceTests
    [TestClass]
    public class LocalVersionServiceTests : TestBase
    {

        //TODO: LocalVersionService is not testable due to use of static classes Directory and File
        //Inject DirectoryService and FileService instead
        [Ignore]
        [TestMethod]
        public void Test_Validate()
        {
            //arrange
            var traceService = new Mock<ITraceService>();

            //act
            var sut = new LocalVersionService(traceService.Object);
            sut.Validate(@"c:\temp\yuniql");

            //assert

        }
    }
}
