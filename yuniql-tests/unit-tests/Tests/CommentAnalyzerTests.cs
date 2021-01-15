using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using Yuniql.Extensibility.SqlBatchParser;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class CommentAnalyzerTests: TestClassBase
    {
        [TestMethod]
        public void Test_Dash_Dash_Inline_Comment()
        {
            //arrange
            var sqlStatementRaw =
$@"SELECT 1;
SELECT 2;
GO

--this is an inline comment
SELECT 3;
GO
";

            //act
            var sut = new CommentAnalyzer();
            var results = sut.Run(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(1);
            results[0].Text.ShouldBe($@"--this is an inline comment" + Environment.NewLine);
            results[0].Start = 28;
            results[0].End = 49;
        }

        [TestMethod]
        public void Test_Dash_Dash_Inline_Comment_After_Sql_Statement()
        {
            //arrange
            var sqlStatementRaw =
$@"SELECT 1;
SELECT 2;
GO

SELECT 3;   --this is an inline comment
GO
";

            //act
            var sut = new CommentAnalyzer();
            var results = sut.Run(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(1);
            results[0].Text.ShouldBe($@"--this is an inline comment" + Environment.NewLine);
            results[0].Start = 40;
            results[0].End = 69;
        }


        [TestMethod]
        public void Test_Dash_Dash_Inline_Comment_Inside_Start_Of_Valid_Sql_Statement()
        {
            //arrange
            var sqlStatementRaw =
$@"SELECT 1;
SELECT 2;
GO

SELECT '--this is an inline comment';
GO
";

            //act
            var sut = new CommentAnalyzer();
            var results = sut.Run(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(0);
        }


        [TestMethod]
        public void Test_Dash_Dash_Inline_Comment_Inside_Middle_Valid_Sql_Statement()
        {
            //arrange
            var sqlStatementRaw =
$@"SELECT 1;
SELECT 2;
GO

SELECT 'This is normal statement with --this is an inline comment';
GO
";

            //act
            var sut = new CommentAnalyzer();
            var results = sut.Run(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(0);
        }

        [TestMethod]
        public void Test_Single_Line_Block_Comment()
        {
            //arrange
            var sqlStatementRaw =
$@"SELECT 1;
SELECT 2;
GO

/*this is a single line block comment*/
SELECT 3;
GO
";

            //act
            var sut = new CommentAnalyzer();
            var results = sut.Run(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(1);
            results[0].Text.ShouldBe($@"/*this is a single line block comment*/");
            results[0].Start = 28;
            results[0].End = 69;
        }

        [TestMethod]
        public void Test_Multi_Line_Block_Comment()
        {
            //arrange
            var sqlStatementRaw =
$@"SELECT 1;
SELECT 2;
GO

/*
another line comment
this is a multi-line block comment
another line comment
*/
SELECT 3;
GO
";

            //act
            var sut = new CommentAnalyzer();
            var results = sut.Run(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(1);
            results[0].Text.ShouldBe(
$@"/*
another line comment
this is a multi-line block comment
another line comment
*/");
            results[0].Start = 28;
            results[0].End = 116;
        }


        [TestMethod]
        public void Test_Nested_Multi_Line_Block_Comment()
        {
            //arrange
            var sqlStatementRaw =
$@"SELECT 1;
SELECT 2;
GO

/*
another line comment
/*this is a multi-line block comment*/
another line comment
*/
SELECT 3;
GO
";

            //act
            var sut = new CommentAnalyzer();
            var results = sut.Run(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(1);
            results[0].Text.ShouldBe(
$@"/*
another line comment
/*this is a multi-line block comment*/
another line comment
*/");
            results[0].Start = 28;
            results[0].End = 120;
        }

        [TestMethod]
        public void Test_Nested_Single_Line_Block_Comment()
        {
            //arrange
            var sqlStatementRaw =
$@"SELECT 1;
SELECT 2;
GO

/*another line comment /*this is a multi-line block comment*/ another line comment*/
SELECT 3;
GO
";

            //act
            var sut = new CommentAnalyzer();
            var results = sut.Run(sqlStatementRaw);

            //assert
            results.Count.ShouldBe(1);
            results[0].Text.ShouldBe(
$@"/*another line comment /*this is a multi-line block comment*/ another line comment*/");
            results[0].Start = 28;
            results[0].End = 114;
        }
    }
}
