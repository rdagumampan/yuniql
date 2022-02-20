using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yuniql.Extensibility;
using Shouldly;
using Yuniql.Core;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class LocalVersionParserTests : TestClassBase
    {
        //https://regex101.com/
        //v1
        //v1.
        //v10.1
        //v10.1.1
        //v10.1.1-label
        //v25102022-label
        //v1.01
        //v1.01.01
        //v1.01.01-label

        //v1
        //v1.1
        //v1.1.1
        //v25102022
        //v01
        //v01.01
        //v01.01.01

        [DataTestMethod]
        [DataRow("v1", 1, 0, 0)]
        [DataRow("v1.2", 1, 2, 0)]
        [DataRow("v1.2.3", 1, 2, 3)]
        [DataRow("v01", 1, 0, 0)]
        [DataRow("v01.02", 1, 2, 0)]
        [DataRow("v01.02.03", 1, 2, 3)]
        [DataRow("v25102022", 25102022, 0, 0)]
        public void Test_Standard_Version_Format(string versionText, int major, int minor, int revision)
        {
            //arrange, act & assert
            var localVersion = new LocalVersion(versionText, string.Empty);
            localVersion.Major.ShouldBe(major);
            localVersion.Minor.ShouldBe(minor);
            localVersion.Revision.ShouldBe(revision);
        }

        [DataTestMethod]
        [DataRow("v1-label", 1, 0, 0, "-label")]
        [DataRow("v1.2-label", 1, 2, 0, "-label")]
        [DataRow("v1.2.3-label", 1, 2, 3, "-label")]
        [DataRow("v01-label", 1, 0, 0, "-label")]
        [DataRow("v01.02-label", 1, 2, 0, "-label")]
        [DataRow("v01.02.03-label", 1, 2, 3, "-label")]
        [DataRow("v25102022-label", 25102022, 0, 0, "-label")]
        public void Test_Version_Format_With_Labels(string versionText, int major, int minor, int revision, string label)
        {
            //arrange, act & assert
            var localVersion = new LocalVersion(versionText, string.Empty);
            localVersion.Major.ShouldBe(major);
            localVersion.Minor.ShouldBe(minor);
            localVersion.Revision.ShouldBe(revision);
            localVersion.Label.ShouldBe(label);
        }

    }
}
