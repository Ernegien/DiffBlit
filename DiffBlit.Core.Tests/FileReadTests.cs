using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiffBlit.Core.Extensions;
using DiffBlit.Core.IO;

namespace DiffBlit.Core.Tests
{
    [TestClass]
    public class FileReadTests
    {
        [TestMethod]
        public void LocalFileReadTest()
        {
            ReadOnlyFile localFile = new ReadOnlyFile(@"C:\Windows\notepad.exe");
            using (ReadOnlyStream s = localFile.Open())
            {
                byte[] data = s.ReadAllBytes();
                Assert.IsTrue(s.Length > 0);
                Assert.AreEqual(data.Length, s.Length);
                Assert.IsFalse(s.CanWrite);
            }
        }

        [TestMethod]
        public void WebFileReadTest()
        {
            ReadOnlyFile webFile = new ReadOnlyFile("https://www.google.com/");
            using (ReadOnlyStream s = webFile.Open())
            using (StreamReader sr = new StreamReader(s))
            {
                string html = sr.ReadToEnd();
                Assert.IsTrue(html.StartsWith("<!doctype html>"));
                Assert.IsFalse(s.CanWrite);
            }
        }

        [TestMethod]
        public void FtpFileReadTest()
        {
            ReadOnlyFile ftpFile = new ReadOnlyFile("ftp://speedtest.tele2.net/1KB.zip");
            using (ReadOnlyStream s = ftpFile.Open())
            {
                byte[] data = s.ReadAllBytes();
                Assert.AreEqual(data.Length, 0x400);
                Assert.IsFalse(s.CanWrite);
            }
        }
    }
}
