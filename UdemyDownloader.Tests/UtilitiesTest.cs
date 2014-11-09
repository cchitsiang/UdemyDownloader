using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UdemyDownloader.Tests
{
    [TestClass]
    public class UtilitiesTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DeploymentItem("TestData\\FilePaths.csv")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "FilePaths.csv", "FilePaths#CSV", DataAccessMethod.Sequential)]
        public void TestNormalizePath()
        {
            var path = Utilities.NormalizeFilePath(TestContext.DataRow[0].ToString());

            Assert.AreEqual(TestContext.DataRow[1].ToString(), path);

        }
    }
}
