using System.Collections.Generic;
using DiffBlit.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiffBlit.Core.Tests
{
    [TestClass]
    public class FileInfoTests
    {
        [TestMethod]
        public void FileInformationEqualityTests()
        {
            var path = "test/path/file.ext";
            var hash = new byte[40].FillRandom();
            var info1 = new FileInformation(path, hash);
            var info2 = new FileInformation(path, hash);
            var info3 = new FileInformation("", null);
            Assert.AreEqual(info1.GetHashCode(), info2.GetHashCode());
            Assert.AreEqual(info1, info2);
            Assert.AreNotEqual(info1.GetHashCode(), info3.GetHashCode());
            Assert.AreNotEqual(info1, info3);

            HashSet<FileInformation> files = new HashSet<FileInformation>();
            files.Add(info1);
            files.Add(info3);
            Assert.IsTrue(files.Contains(info1));
            Assert.IsTrue(files.Contains(info2));
            Assert.IsTrue(files.Contains(info1));

        }
    }
}
