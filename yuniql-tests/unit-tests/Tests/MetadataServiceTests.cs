using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data;
using Yuniql.Core;
using Yuniql.Extensibility;
using Yuniql.SqlServer;

namespace Yuniql.UnitTests
{
    //TODO: Implement MetadataServiceTests
    [TestClass]
    public class MetadataServiceTests : TestClassBase
    {
        //TODO: MetadataService is not testable due to use of extension methods
        [Ignore]
        [TestMethod]
        public void Test_IsDatabaseExists()
        {
            //arrange
            var connection = new Mock<IDbConnection>();
            //connection.Setup(s=> s.QuerySingleBool())

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.GetConnectionInfo()).Returns(new ConnectionInfo { Database = "helloyuniql" });
            dataService.Setup(s => s.CreateMasterConnection()).Returns(connection.Object);

            var tokens = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_DB_NAME, "helloyuniql") };

            var tokenReplacementService = new Mock<ITokenReplacementService>();
            tokenReplacementService.Setup(s => s.Replace(It.IsAny<List<KeyValuePair<string, string>>>(), @"SELECT ISNULL(database_id, 0) FROM [sys].[databases] WHERE name = '${YUNIQL_DB_NAME}'"));

            var traceService = new Mock<ITraceService>();

            //act
            var sut = new MetadataService(dataService.Object, traceService.Object, tokenReplacementService.Object);
            sut.IsDatabaseExists();

            //assert
        }

    }
}
