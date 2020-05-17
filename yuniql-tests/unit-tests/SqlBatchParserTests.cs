using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using Yuniql.Extensibility;
using Yuniql.Extensibility.SqlBatchParser;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class SqlBatchParserTests
    {
        [TestMethod]
        public void Test_Init_Option_No_Explicit_Options()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw = $@"
--GO inline comment
CREATE PROC table1
AS
    SELECT 1;
GO

/*
GO in inline comment block
*/

CREATE PROC table2
AS
    SELECT 1;
GO

/* multiline comment block
GO
*/

CREATE PROC table3
AS
    SELECT 1;
";

            //act
            var sut = new SqlBatchParser(traceService.Object, new GoSqlBatchLineAnalyzer(), new CStyleCommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(3);
        }
    }
}
