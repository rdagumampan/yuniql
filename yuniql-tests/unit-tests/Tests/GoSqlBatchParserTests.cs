using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using System.Linq;
using Yuniql.Extensibility;
using Yuniql.Extensibility.SqlBatchParser;

namespace Yuniql.UnitTests
{

    [TestClass]
    public class GoSqlBatchParserTests: TestClassBase
    {
        [TestMethod]
        public void Test_Go_Basic_Two_Batches()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"SELECT 1;
SELECT 2;
GO

SELECT 3;
GO
";
            //act
            var sut = new SqlBatchParser(traceService.Object, new GoSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results[0].BatchText.ShouldBe(
    $@"SELECT 1;
SELECT 2;");
            results[1].BatchText.ShouldBe(
    $@"SELECT 3;");
        }

        [TestMethod]
        public void Test_Go_Without_Go_In_Last_Line()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"SELECT 1;
SELECT 2;
GO

SELECT 3;
";
            //act
            var sut = new SqlBatchParser(traceService.Object, new GoSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results[0].BatchText.ShouldBe(
    $@"SELECT 1;
SELECT 2;");
            results[1].BatchText.ShouldBe(
    $@"SELECT 3;");
        }

        [TestMethod]
        public void Test_Go_Insidide_Sql_Statement_Literal()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"SELECT 1;
SELECT 'This is GO inside valid sql statement';
GO

SELECT 3;
GO
";
            //act
            var sut = new SqlBatchParser(traceService.Object, new GoSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results[0].BatchText.ShouldBe(
    $@"SELECT 1;
SELECT 'This is GO inside valid sql statement';");
            results[1].BatchText.ShouldBe(
    $@"SELECT 3;");
        }

        [TestMethod]
        public void Test_Go_Inside_Inline_Comment()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"--GO inline comment
SELECT 1;
SELECT 2;
GO

SELECT 3;
GO
";

            //act
            var sut = new SqlBatchParser(traceService.Object, new GoSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results[0].BatchText.ShouldBe(
    $@"--GO inline comment
SELECT 1;
SELECT 2;");
            results[1].BatchText.ShouldBe(
    $@"SELECT 3;");
        }

        [TestMethod]
        public void Test_Go_Inside_Singleline_Block_Comment()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"/*GO in inline comment block*/
SELECT 1;
SELECT 2;
GO

SELECT 3;
GO
";
            //act
            var sut = new SqlBatchParser(traceService.Object, new GoSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results[0].BatchText.ShouldBe(
    $@"/*GO in inline comment block*/
SELECT 1;
SELECT 2;");
            results[1].BatchText.ShouldBe(
    $@"SELECT 3;");
        }


        [TestMethod]
        public void Test_Go_Inside_Multiline_Block_Comment()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"/* multiline comment block
GO
*/
SELECT 1;
SELECT 2;
GO

SELECT 3;
GO
";

            //act
            var sut = new SqlBatchParser(traceService.Object, new GoSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results.First().BatchText.ShouldBe(
    $@"/* multiline comment block
GO
*/
SELECT 1;
SELECT 2;");
            results[1].BatchText.ShouldBe(
    $@"SELECT 3;");
        }


        [TestMethod]
        public void Test_Go_Inside_Nested_Multiline_Block_Comment()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"/* multiline comment block
/*
GO
*/
*/
SELECT 1;
SELECT 2;
GO

SELECT 3;
GO
";

            //act
            var sut = new SqlBatchParser(traceService.Object, new GoSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results.First().BatchText.ShouldBe(
    $@"/* multiline comment block
/*
GO
*/
*/
SELECT 1;
SELECT 2;");
            results[1].BatchText.ShouldBe(
    $@"SELECT 3;");
        }


    }
}
