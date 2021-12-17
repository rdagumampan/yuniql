using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yuniql.Extensibility;
using Shouldly;
using System;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class StringExtensionsTests : TestClassBase
    {

        [TestMethod]
        public void Test_Quote()
        {
            //arrange, act & assert
            "mytext".Quote().ShouldBe("'mytext'");
        }

        [TestMethod]
        public void Test_IsSingleQuoted()
        {
            //arrange, act & assert
            "'mytext'".IsSingleQuoted().ShouldBeTrue();
        }

        public void Test_DoubleQuote()
        {
            //arrange, act & assert
            "mytext".DoubleQuote().ShouldBe("\"mytext\"");
        }

        [TestMethod]
        public void Test_IsDoubleQuoted()
        {
            //arrange, act & assert
            "\"mytext\"".IsDoubleQuoted().ShouldBeTrue();
        }

        [TestMethod]
        public void Test_UnQuote_Single_Quoted()
        {
            //arrange, act & assert
            "'mytext'".UnQuote().ShouldBe("mytext");
        }

        [TestMethod]
        public void Test_UnQuote_Double_Quoted()
        {
            //arrange, act & assert
            "\"mytext\"".UnQuote().ShouldBe("mytext");
        }

        [TestMethod]
        public void Test_Escape()
        {
            //arrange, act & assert
            @"c:\temp\yuniql".Escape().ShouldBe(@"c:\\temp\\yuniql");
        }

        [TestMethod]
        public void Test_Unescape()
        {
            //arrange, act & assert
            @"c:\\temp\\yuniql".Unescape().ShouldBe(@"c:\temp\yuniql");
        }


        [TestMethod]
        public void Test_SplitBulkFileName_mysequenceno_myschema_mytable_whatever_Throws_Exception()
        {
            //arrange & act
            var exception = Assert.ThrowsException<ArgumentException>(() =>
            {
                var result = "1.myschema.mytable.whatever".SplitBulkFileName(defaultSchema: "dbo");
            });

            //assert
            exception.Message.Contains("Bulk file name must have maximum 3 segments.");
            exception.Message.Contains("These are the valid file name patterns: 1.myschema.mytable.csv, 01.myschema.mytable.csv, 1.mytable.csv, 01.mytable.csv, myschema.mytable.csv, mytable.csv.");
        }

        [TestMethod]
        public void Test_SplitBulkFileName_mysequenceno_myschema_mytable()
        {
            //arrange & act
            var result = "1.myschema.mytable".SplitBulkFileName(defaultSchema: "dbo");

            //assert
            result.Item1.ShouldBe("1");
            result.Item2.ShouldBe("myschema");
            result.Item3.ShouldBe("mytable");
        }

        [TestMethod]
        public void Test_SplitBulkFileName_xx_mysequenceno_myschema_mytable()
        {
            //arrange & act
            var result = "01.myschema.mytable".SplitBulkFileName(defaultSchema: "dbo");

            //assert
            result.Item1.ShouldBe("01");
            result.Item2.ShouldBe("myschema");
            result.Item3.ShouldBe("mytable");
        }

        [TestMethod]
        public void Test_SplitBulkFileName_myschema_mytable()
        {
            //arrange & act
            var result = "myschema.mytable".SplitBulkFileName(defaultSchema: "dbo");

            //assert
            result.Item1.ShouldBe(string.Empty);
            result.Item2.ShouldBe("myschema");
            result.Item3.ShouldBe("mytable");
        }

        [TestMethod]
        public void Test_SplitBulkFileName_1_mytable()
        {
            //arrange & act
            var result = "1.mytable".SplitBulkFileName(defaultSchema: "dbo");

            //assert
            result.Item1.ShouldBe("1");
            result.Item2.ShouldBe("dbo");
            result.Item3.ShouldBe("mytable");
        }

        [TestMethod]
        public void Test_SplitBulkFileName_01_mytable()
        {
            //arrange & act
            var result = "01.mytable".SplitBulkFileName(defaultSchema: "dbo");

            //assert
            result.Item1.ShouldBe("01");
            result.Item2.ShouldBe("dbo");
            result.Item3.ShouldBe("mytable");
        }

        [TestMethod]
        public void Test_SplitBulkFileName_mytable()
        {
            //arrange & act
            var result = "mytable".SplitBulkFileName(defaultSchema: "dbo");

            //assert
            result.Item1.ShouldBe(string.Empty);
            result.Item2.ShouldBe("dbo");
            result.Item3.ShouldBe("mytable");
        }

        [TestMethod]
        public void Test_HasUpper_MyTable()
        {
            //arrange & act
            var result = "MyTable".HasUpper();

            //assert
            result.ShouldBeTrue();
        }

        [TestMethod]
        public void Test_HasUpper_mytable()
        {
            //arrange & act
            var result = "mytable".HasUpper();

            //assert
            result.ShouldBeFalse();
        }

    }
}
