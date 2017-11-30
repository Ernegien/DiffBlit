using System;
using System.IO;
using System.Linq;
using DiffBlit.Core.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiffBlit.Core.Tests
{
    [TestClass]
    public class ConfigParseTests
    {
        [TestMethod]
        public void RepoSerializationTest()
        {
            var repo = Repository.Deserialize(File.ReadAllText("content\\repo.json"));
            string json = repo.Serialize();
            var repo2 = Repository.Deserialize(json);
        }

        [TestMethod]
        public void SnapshotCreationTest()
        {
            //var repo = Repository.Deserialize(File.ReadAllText("content\\repo.json"));

            //var snap = Snapshot.Create(repo, Environment.CurrentDirectory, "test name");
            //snap.Version = new Version();

            //string json = repo.Serialize();
        }

        [TestMethod]
        public void PackageCreationTest()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDirectory);

            try
            {
                var repo = new Repository();
                repo.Name = "Test Repo Name";
                repo.Description = "Test Repo Description";

                string sourceContentPath = Path.Combine(Environment.CurrentDirectory, "content\\source");
                string targetContentPath = Path.Combine(Environment.CurrentDirectory, "content\\target");

                // generate package, get associated snapshots
                var package = Package.Create(repo, sourceContentPath, targetContentPath, tempDirectory);
                var sourceSnapshot = repo.Snapshots.Select(s => s.Id == package.SourceSnapshotId);
                var targetSnapshot = repo.Snapshots.Select(s => s.Id == package.TargetSnapshotId);
                repo.Packages.Add(package);

                string json = repo.Serialize();


                //foreach (var action in package.Actions)
                //{
                //    action.Run();   // TODO: specify input and output directories?
                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                Directory.Delete(tempDirectory, true);
            }
        }

    }
}
