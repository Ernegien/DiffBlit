using Microsoft.VisualStudio.TestTools.UnitTesting;
using Path = DiffBlit.Core.IO.Path;

namespace DiffBlit.Core.Tests
{
    [TestClass]
    public class PathTests
    {
        // NOTE: absolute paths without a trailing slash should be interpretted as extensionless files, unless it's the root

        [TestMethod]
        public void InvalidPathTest()
        {

            // TODO: invalid
            //path = new Path(null);
            //path = new Path(string.Empty);
            //path = new Path("/");
            //path = new Path("\\");
            //path = new Path(@"C:");


        }

        [TestMethod]
        public void RelativePathTest()
        {
            Path path;

            path = new Path("file");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsFalse(path.IsAbsolute);

            path = new Path("/file");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsFalse(path.IsAbsolute);

            path = new Path("file.ext");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsFalse(path.IsAbsolute);

            path = new Path("/file.ext");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsFalse(path.IsAbsolute);

            path = new Path("directory/");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsFalse(path.IsAbsolute);

            path = new Path("/directory/");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsFalse(path.IsAbsolute);
        }

        [TestMethod]
        public void LocalPathTest()
        {
            Path path;

            path = new Path(@"C:\");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"C:\file");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"C:\file.ext");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"C:\directory\");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            // alternate separators are fine
            path = new Path(@"C:/directory/");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);
        }

        [TestMethod]
        public void HttpPathTest()
        {
            Path path;

            path = new Path(@"http://localhost");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"http://localhost/");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"http://localhost/file");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"http://localhost/file.ext");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"http://localhost/directory/");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);
        }

        [TestMethod]
        public void HttpsPathTest()
        {
            Path path;

            path = new Path(@"https://localhost");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"https://localhost/");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"https://localhost/file");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"https://localhost/file.ext");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"https://localhost/directory/");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);
        }

        [TestMethod]
        public void FtpPathTest()
        {
            Path path;

            path = new Path(@"ftp://localhost");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"ftp://localhost/");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"ftp://localhost/file");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"ftp://localhost/file.ext");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"ftp://localhost/directory/");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);
        }

        [TestMethod]
        public void UncPathTest()
        {
            Path path;

            path = new Path(@"\\localhost");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"\\localhost\");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"\\localhost\file");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"\\localhost\file.ext");
            Assert.IsFalse(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);

            path = new Path(@"\\localhost\directory\");
            Assert.IsTrue(path.IsDirectory);
            Assert.IsTrue(path.IsAbsolute);
        }

        [TestMethod]
        public void PathEqualityTest()
        {
            Assert.AreEqual(new Path("test"), new Path("test"));
            Assert.AreEqual(new Path("TEST"), new Path("TEST"));
            Assert.AreEqual(new Path("test"), new Path("Test"));
            Assert.AreEqual(new Path("test/"), new Path("Test/"));
            Assert.AreEqual(new Path("test.bin"), new Path("Test.bin"));

            Assert.AreNotEqual(new Path("test", true), new Path("Test"));
            Assert.AreNotEqual(new Path("test"), new Path("Test", true));
            Assert.AreNotEqual(new Path("test", true), new Path("Test", true));

            Assert.AreEqual(new Path("test", true).GetHashCode(), "test".GetHashCode());
            Assert.AreNotEqual(new Path("test").GetHashCode(), "test".GetHashCode());
            Assert.AreNotEqual(new Path("TEST").GetHashCode(), "test".GetHashCode());
        }

        [TestMethod]
        public void PathDirectoryNameTest()
        {
            Assert.IsNull(Path.GetDirectoryName(@"file"));
            Assert.IsNull(Path.GetDirectoryName(@"file.ext"));
            Assert.AreEqual(Path.GetDirectoryName(@"directory\"), new Path(@"directory\"));
            Assert.AreEqual(Path.GetDirectoryName(@"directory\file"), new Path(@"directory\"));
            Assert.AreEqual(Path.GetDirectoryName(@"directory\file.ext"), new Path(@"directory\"));

            Assert.AreEqual(Path.GetDirectoryName(@"C:\file"), new Path(@"C:\"));
            Assert.AreEqual(Path.GetDirectoryName(@"C:\file.ext"), new Path(@"C:\"));
            Assert.AreEqual(Path.GetDirectoryName(@"C:\directory\"), new Path(@"C:\directory\"));
            Assert.AreEqual(Path.GetDirectoryName(@"C:\directory\file"), new Path(@"C:\directory\"));
            Assert.AreEqual(Path.GetDirectoryName(@"C:\directory\file.ext"), new Path(@"C:\directory\"));

            Assert.AreEqual(Path.GetDirectoryName(@"http://example.com/file"), new Path(@"http://example.com/"));
            Assert.AreEqual(Path.GetDirectoryName(@"http://example.com/file.ext"), new Path(@"http://example.com/"));
            Assert.AreEqual(Path.GetDirectoryName(@"http://example.com/directory/"), new Path(@"http://example.com/directory/"));
            Assert.AreEqual(Path.GetDirectoryName(@"http://example.com/directory/file"), new Path(@"http://example.com/directory/"));
            Assert.AreEqual(Path.GetDirectoryName(@"http://example.com/directory/file.ext"), new Path(@"http://example.com/directory/"));
        }

        [TestMethod]
        public void PathFileNameTest()
        {
            Assert.AreEqual(Path.GetFileName(@"file"), new Path(@"file"));
            Assert.AreEqual(Path.GetFileName(@"file.ext"), new Path(@"file.ext"));
            Assert.IsNull(Path.GetFileName(@"directory\"));
            Assert.AreEqual(Path.GetFileName(@"directory\file"), new Path(@"file"));
            Assert.AreEqual(Path.GetFileName(@"directory\file.ext"), new Path(@"file.ext"));

            Assert.AreEqual(Path.GetFileName(@"C:\file"), new Path(@"file"));
            Assert.AreEqual(Path.GetFileName(@"C:\file.ext"), new Path(@"file.ext"));
            Assert.IsNull(Path.GetFileName(@"C:\directory\"));
            Assert.AreEqual(Path.GetFileName(@"C:\directory\file"), new Path(@"file"));
            Assert.AreEqual(Path.GetFileName(@"C:\directory\file.ext"), new Path(@"file.ext"));

            // TODO: convert the others to this format
            Assert.AreEqual("file", Path.GetFileName(@"http://example.com/file").Name);
            Assert.AreEqual("file.ext", Path.GetFileName(@"http://example.com/file.ext").Name);
            Assert.IsNull(Path.GetFileName(@"http://example.com/directory/"));
            Assert.AreEqual("file", Path.GetFileName(@"http://example.com/directory/file").Name);
            Assert.AreEqual("file.ext", Path.GetFileName(@"http://example.com/directory/file.ext").Name);
        }

        [TestMethod]
        public void PathCombineTest()
        {
            var test = Path.Combine("http://example.com/", "file");
            Assert.AreEqual(Path.Combine("http://example.com/", "file"), new Path("http://example.com/file"));
            Assert.AreEqual(Path.Combine("http://example.com/directory/", "file"), new Path("http://example.com/directory/file"));


        }
    }
}
