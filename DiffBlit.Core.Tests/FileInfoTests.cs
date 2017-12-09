using System;
using DiffBlit.Core.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiffBlit.Core.Tests
{
    [TestClass]
    public class FileInfoTests
    {
        [TestMethod]
        public void ConstructionTests()
        {
            var info = new FileInformation();

            // empty absolute
            info = new FileInformation("/");
            info = new FileInformation("\\");

            info = new FileInformation("file"); // root extensionless file
            Assert.IsFalse(info.Path.IsDirectory);
            Assert.IsFalse(info.Path.IsAbsolute);

            info = new FileInformation("file.dat"); // root file
            Assert.IsFalse(info.Path.IsDirectory);
            Assert.IsFalse(info.Path.IsAbsolute);

            info = new FileInformation("directory/");    // root directory
            Assert.IsTrue(info.Path.IsDirectory);
            Assert.IsFalse(info.Path.IsAbsolute);

            //info = new FileInformation("http://example.com"); // directory?
            //Assert.IsTrue(info.IsDirectory);
            //Assert.IsTrue(info.IsAbsolute);

            info = new FileInformation("http://example.com/directory/");
            Assert.IsTrue(info.Path.IsDirectory);
            Assert.IsTrue(info.Path.IsAbsolute);

            info = new FileInformation("https://example.com/directory/");
            Assert.IsTrue(info.Path.IsDirectory);
            Assert.IsTrue(info.Path.IsAbsolute);

            info = new FileInformation("http://example.com/file");
            Assert.IsFalse(info.Path.IsDirectory);
            Assert.IsTrue(info.Path.IsAbsolute);

            info = new FileInformation("http://example.com/file.dat");
            Assert.IsFalse(info.Path.IsDirectory);
            Assert.IsTrue(info.Path.IsAbsolute);

            info = new FileInformation("C:\\");
            Assert.IsTrue(info.Path.IsDirectory);
            Assert.IsTrue(info.Path.IsAbsolute);

            info = new FileInformation("C:\\file.dat");
            Assert.IsFalse(info.Path.IsDirectory);
            Assert.IsTrue(info.Path.IsAbsolute);

            info = new FileInformation("\\\\127.0.0.1\\file.dat");
            Assert.IsFalse(info.Path.IsDirectory);
            Assert.IsTrue(info.Path.IsAbsolute);

            info = new FileInformation("/file.dat");
            Assert.IsFalse(info.Path.IsDirectory);
            Assert.IsFalse(info.Path.IsAbsolute);

            info = new FileInformation("\\file.dat");
            Assert.IsFalse(info.Path.IsDirectory);
            Assert.IsFalse(info.Path.IsAbsolute);

            info = new FileInformation("/directory/");
            Assert.IsTrue(info.Path.IsDirectory);
            Assert.IsFalse(info.Path.IsAbsolute);

            info = new FileInformation("\\directory\\");
            Assert.IsTrue(info.Path.IsDirectory);
            Assert.IsFalse(info.Path.IsAbsolute);

            //Assert.ThrowsException<ArgumentException>(() => new FileInformation(null));
            //Assert.ThrowsException<ArgumentException>(() => new FileInformation(string.Empty));
            //Assert.ThrowsException<ArgumentException>(() => new FileInformation("\\\\\\test"));
            //Assert.ThrowsException<ArgumentException>(() => new FileInformation("%"));


        }

        [TestMethod]
        public void FilePathTests()
        {
            Assert.AreEqual(new FilePath("test"), new FilePath("test"));
            Assert.AreEqual(new FilePath("TEST"), new FilePath("TEST"));
            Assert.AreEqual(new FilePath("test"), new FilePath("Test"));
            Assert.AreEqual(new FilePath("test/"), new FilePath("Test/"));
            Assert.AreEqual(new FilePath("test.bin"), new FilePath("Test.bin"));

            Assert.AreNotEqual(new FilePath("test", true), new FilePath("Test"));
            Assert.AreNotEqual(new FilePath("test"), new FilePath("Test", true));
            Assert.AreNotEqual(new FilePath("test", true), new FilePath("Test", true));

            Assert.AreEqual(new FilePath("test", true).GetHashCode(), "test".GetHashCode());
            Assert.AreNotEqual(new FilePath("test").GetHashCode(), "test".GetHashCode());
            Assert.AreNotEqual(new FilePath("TEST").GetHashCode(), "test".GetHashCode());


            // TODO: 
        }
    }
}
