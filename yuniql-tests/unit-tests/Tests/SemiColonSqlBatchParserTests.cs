using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using System.Linq;
using Yuniql.Extensibility;
using Yuniql.Extensibility.SqlBatchParser;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class SemiColonSqlBatchParserTests: TestClassBase
    {
        [TestMethod]
        public void Test_SemiColon_Basic_Two_Batches()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"SELECT 1
SELECT 2;
SELECT 3;
";

            //act
            var sut = new SqlBatchParser(traceService.Object, new SemiColonSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results[0].BatchText.ShouldBe($@"SELECT 1
SELECT 2;");
            results[1].BatchText.ShouldBe($@"SELECT 3;");
        }

        [TestMethod]
        public void Test_SemiColon_Without_SemiColon_In_Last_Line()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"SELECT 1
SELECT 2;

SELECT 3
";
            //act
            var sut = new SqlBatchParser(traceService.Object, new SemiColonSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results[0].BatchText.ShouldBe(
    $@"SELECT 1
SELECT 2;");
            results[1].BatchText.ShouldBe(
    $@"SELECT 3");
        }

        [TestMethod]
        public void Test_SemiColon_Inside_Sql_Statement_Literal()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"SELECT 1
SELECT 'This is ; inside valid sql statement';

SELECT 3;
";
            //act
            var sut = new SqlBatchParser(traceService.Object, new SemiColonSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results[0].BatchText.ShouldBe(
    $@"SELECT 1
SELECT 'This is ; inside valid sql statement';");
            results[1].BatchText.ShouldBe(
    $@"SELECT 3;");
        }

        [TestMethod]
        public void Test_SemiColon_Inside_Inline_Comment()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"--; inline comment
SELECT 1
SELECT 2;

SELECT 3;
";

            //act
            var sut = new SqlBatchParser(traceService.Object, new SemiColonSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results[0].BatchText.ShouldBe(
    $@"--; inline comment
SELECT 1
SELECT 2;");
            results[1].BatchText.ShouldBe(
    $@"SELECT 3;");
        }

        [TestMethod]
        public void Test_SemiColon_Inside_Singleline_Block_Comment()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"/*; in inline comment block*/
SELECT 1
SELECT 2;

SELECT 3;
";
            //act
            var sut = new SqlBatchParser(traceService.Object, new SemiColonSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results[0].BatchText.ShouldBe(
    $@"/*; in inline comment block*/
SELECT 1
SELECT 2;");
            results[1].BatchText.ShouldBe(
    $@"SELECT 3;");
        }


        [TestMethod]
        public void Test_SemiColon_Inside_Multiline_Block_Comment()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"/* multiline comment block
;
*/
SELECT 1
SELECT 2;

SELECT 3;
";

            //act
            var sut = new SqlBatchParser(traceService.Object, new SemiColonSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results.First().BatchText.ShouldBe(
    $@"/* multiline comment block
;
*/
SELECT 1
SELECT 2;");
            results[1].BatchText.ShouldBe(
    $@"SELECT 3;");
        }


        [TestMethod]
        public void Test_SemiColon_Inside_Nested_Multiline_Block_Comment()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var sqlStatementRaw =
    $@"/* multiline comment block
/*
;
*/
*/
SELECT 1
SELECT 2;

SELECT 3;
";

            //act
            var sut = new SqlBatchParser(traceService.Object, new SemiColonSqlBatchLineAnalyzer(), new CommentAnalyzer());
            var results = sut.Parse(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(2);
            results.First().BatchText.ShouldBe(
    $@"/* multiline comment block
/*
;
*/
*/
SELECT 1
SELECT 2;");
            results[1].BatchText.ShouldBe(
    $@"SELECT 3;");
        }
    }
}
