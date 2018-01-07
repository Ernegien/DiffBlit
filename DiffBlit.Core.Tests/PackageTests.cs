using System;
using System.IO;
using DiffBlit.Core.Config;
using DiffBlit.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Path = DiffBlit.Core.IO.Path;

namespace DiffBlit.Core.Tests
{
    [TestClass]
    public class PackageTests
    {
        // TODO: break this down into multiple tests
        [TestMethod]
        public void PackageCreateApplySerializeTest()
        {
            // create temp directories to be used during patching
            Path tempDirectory = Utility.GetTempDirectory();
            Directory.CreateDirectory(tempDirectory);
            Path tempSourceCopyDirectory = Utility.GetTempDirectory();
            Directory.CreateDirectory(tempSourceCopyDirectory);

            try
            {
                // create a new repo
                var repo = new Repository();
                repo.Name = "Test Repo Name";
                repo.Description = "Test Repo Description";

                // generate package content
                Path sourceContentPath = Path.Combine(Environment.CurrentDirectory + "\\", "content\\source\\");
                Path targetContentPath = Path.Combine(Environment.CurrentDirectory + "\\", "content\\target\\");
                var settings = new PackageSettings(partSize: 40); // test multipart logic
                var package = new Package(repo, sourceContentPath, targetContentPath, tempDirectory, settings);
                package.Name = "Initial Update";

                // update the source snapshot info
                package.SourceSnapshot.Name = "Test Original Name";
                package.SourceSnapshot.Version = new Version("1.0.0.0");

                // update the target snapshot info
                package.TargetSnapshot.Name = "Test Updated Name";
                package.TargetSnapshot.Version = new Version("1.1.0.0");

                // bind the new package to the repo
                repo.Packages.Add(package);

                // test serialization
                string json = repo.Serialize();
                Repository testRepo = Repository.Deserialize(json);
                string json2 = testRepo.Serialize();
                Assert.AreEqual(json, json2);

                // copy source directory to temp directory to patch against
                new Microsoft.VisualBasic.Devices.Computer().FileSystem.CopyDirectory(sourceContentPath, tempSourceCopyDirectory, true);

                // run the actions targeting the temp package directory
                package.Apply(Path.Combine(tempDirectory, package.Id + "\\"), tempSourceCopyDirectory);

                // find method tests (not comprehensive)
                Assert.AreEqual(repo.FindSnapshotFromDirectory(sourceContentPath), package.SourceSnapshot);
                Assert.AreEqual(repo.FindSnapshotFromDirectory(targetContentPath), package.TargetSnapshot);
            }
            finally
            {
                // clean up temp directories
                Directory.Delete(tempDirectory, true);
                Directory.Delete(tempSourceCopyDirectory, true);
            }
        }
    }
}
