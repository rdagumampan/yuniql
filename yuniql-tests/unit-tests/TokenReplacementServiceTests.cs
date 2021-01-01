using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Shouldly;
using Yuniql.Core;
using Moq;
using Yuniql.Extensibility;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class TokenReplacementServiceTests: TestClassBase
    {
        [TestMethod]
        public void Test_Replace_Token()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatement = @"SELECT 'Ok' ${Token1}, 'Ok' ${Token2}, 'Ok' ${Token3}";

            //act
            var sut = new TokenReplacementService(traceService.Object);
            var result = sut.Replace(new List<KeyValuePair<string, string>> {
                 new KeyValuePair<string, string>("Token1","TokenValue1"),
                 new KeyValuePair<string, string>("Token2","TokenValue2"),
                 new KeyValuePair<string, string>("Token3","TokenValue3"),
            }, sqlStatement);

            //asset
            result.ShouldBe("SELECT 'Ok' TokenValue1, 'Ok' TokenValue2, 'Ok' TokenValue3");
        }

        [TestMethod]
        public void Test_Missing_Token_Values_Must_Throw_Exception()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatement = @"SELECT 'Ok' ${Token1}, 'Ok' ${Token2}, 'Ok' ${Token3}";

            //act and assert
            Assert.ThrowsException<YuniqlMigrationException>(() =>
            {
                var sut = new TokenReplacementService(traceService.Object);
                var result = sut.Replace(new List<KeyValuePair<string, string>>(), sqlStatement);
            }).Message.Contains("Some tokens were not successfully replaced.").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Insufficient_Token_Values_Must_Throw_Exception()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatement = @"SELECT 'Ok' ${Token1}, 'Ok' ${Token2}, 'Ok' ${Token3}";

            //act and assert
            Assert.ThrowsException<YuniqlMigrationException>(() =>
            {
                var sut = new TokenReplacementService(traceService.Object);
                var result = sut.Replace(new List<KeyValuePair<string, string>> {
                 new KeyValuePair<string, string>("Token1","TokenValue1"),
                 new KeyValuePair<string, string>("Token2","TokenValue2")
                }, sqlStatement);
            }).Message.Contains("Some tokens were not successfully replaced.").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_When_No_Tokens_Values_Passed_Do_Nothing()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatement = @"SELECT Column1, Column2, Column3";

            //act
            var sut = new TokenReplacementService(traceService.Object);
            var result = sut.Replace(new List<KeyValuePair<string, string>>(), sqlStatement);

            //asset
            result.ShouldBe("SELECT Column1, Column2, Column3");
        }

        [TestMethod]
        public void Test_When_No_Tokens_Found_Do_Nothing()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatement = @"SELECT Column1, Column2, Column3";

            //act
            var sut = new TokenReplacementService(traceService.Object);
            var result = sut.Replace(new List<KeyValuePair<string, string>> {
                 new KeyValuePair<string, string>("Token1","TokenValue1"),
                 new KeyValuePair<string, string>("Token2","TokenValue2"),
                 new KeyValuePair<string, string>("Token3","TokenValue3"),
            }, sqlStatement);

            //asset
            result.ShouldBe("SELECT Column1, Column2, Column3");
        }
    }
}
