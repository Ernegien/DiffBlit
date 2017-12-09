using System;
using System.IO;
using System.Linq;
using DiffBlit.Core.Config;
using DiffBlit.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiffBlit.Core.Tests
{
    [TestClass]
    public class PackageTests
    {
        [TestMethod]
        public void PackageCreateApplySerializeTest()
        {
            // create temp directories to be used during patching
            FilePath tempDirectory = Utility.GetTempDirectory();
            Directory.CreateDirectory(tempDirectory);
            FilePath tempSourceCopyDirectory = Utility.GetTempDirectory();
            Directory.CreateDirectory(tempSourceCopyDirectory);

            try
            {
                // create a new repo
                var repo = new Repository();
                repo.Name = "Test Repo Name";
                repo.Description = "Test Repo Description";

                // generate package content
                FilePath sourceContentPath = Path.Combine(Environment.CurrentDirectory, "content\\source");
                FilePath targetContentPath = Path.Combine(Environment.CurrentDirectory, "content\\target");
                var settings = new PackageSettings(partSize: 40); // test multipart logic
                var package = Package.Create(repo, sourceContentPath, targetContentPath, tempDirectory, settings);
                package.Name = "Initial Update";

                // update the source snapshot info
                var sourceSnapshot = repo.Snapshots.First(s => s.Id == package.SourceSnapshotId);
                sourceSnapshot.Name = "Test Original Name";
                sourceSnapshot.Version = new Version("1.0.0.0");

                // update the target snapshot info
                var targetSnapshot = repo.Snapshots.First(s => s.Id == package.TargetSnapshotId);
                targetSnapshot.Name = "Test Updated Name";
                targetSnapshot.Version = new Version("1.1.0.0");

                // bind the new package to the repo
                repo.Packages.Add(package);

                // test serialization
                string json = repo.Serialize();

                // copy source directory to temp directory to patch against
                new Microsoft.VisualBasic.Devices.Computer().FileSystem.CopyDirectory(sourceContentPath, tempSourceCopyDirectory, true);

                // run the actions targeting the temp package directory
                // TODO: need to figure out best way to embed hidden parent references into deserialized json objects, would be nice to not have to pass snapshots as args etc.
                package.Apply(sourceSnapshot, targetSnapshot, Path.Combine(tempDirectory, package.Id.ToString()), tempSourceCopyDirectory);

                // find method tests (not comprehensive)
                Assert.AreEqual(repo.FindSnapshotFromDirectory(sourceContentPath), sourceSnapshot);
                Assert.AreEqual(repo.FindSnapshotFromDirectory(targetContentPath), targetSnapshot);
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
