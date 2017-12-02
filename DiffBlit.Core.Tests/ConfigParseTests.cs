using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiffBlit.Core.Config;
using DiffBlit.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiffBlit.Core.Tests
{
    [TestClass]
    public class ConfigParseTests
    {
        [TestMethod]
        public void RepoSerializationTest()
        {
            //var repo = Repository.Deserialize(File.ReadAllText("content\\repo.json"));
            //string json = repo.Serialize();
            //var repo2 = Repository.Deserialize(json);
        }

        [TestMethod]
        public void SnapshotCreationTest()
        {
            // TODO: update repo.json
            //var repo = Repository.Deserialize(File.ReadAllText("content\\repo.json"));
            //var snapshot = Snapshot.Create(Environment.CurrentDirectory);
            //snapshot.Version = new Version();
            //snapshot.Name = "Test Snapshot";
            //repo.Snapshots.Add(snapshot);
            //string json = repo.Serialize();
        }

        // TODO: break up into multiple tests, create Package.Apply method etc.
        [TestMethod]
        public void PackageCreationTest()
        {
            var tempDirectory = Utility.GetTempDirectory();
            Directory.CreateDirectory(tempDirectory);
            string tempSourceCopyDirectory = Utility.GetTempDirectory();
            Directory.CreateDirectory(tempSourceCopyDirectory);

            try
            {
                // create a new repo
                var repo = new Repository();
                repo.Name = "Test Repo Name";
                repo.Description = "Test Repo Description";

                // generate package content
                string sourceContentPath = Path.Combine(Environment.CurrentDirectory, "content\\source");
                string targetContentPath = Path.Combine(Environment.CurrentDirectory, "content\\target");
                var settings = new PackageSettings(16); // test multiple parts with low chunk size
                settings.CompressionEnabled = false;    // TODO: temporary until it's implemented in content save method
                var package = Package.Create(repo, sourceContentPath, targetContentPath, tempDirectory, settings);
                package.Name = "Initial Update";

                // update the source snapshot info
                var sourceSnapshot = repo.Snapshots.First(s => s.Id == package.SourceSnapshotId);
                sourceSnapshot.Name = "Test Original Name";

                // update the target snapshot info
                var targetSnapshot = repo.Snapshots.First(s => s.Id == package.TargetSnapshotId);
                targetSnapshot.Name = "Test Updated Name";

                // bind the new package to the repo
                repo.Packages.Add(package);

                // test serialization
                string json = repo.Serialize();

                // copy source directory to temp directory to patch against
                new Microsoft.VisualBasic.Devices.Computer().FileSystem.CopyDirectory(sourceContentPath, tempSourceCopyDirectory, true);

                // run the actions targeting the temp package directory
                string tempPackageDirectory = Path.Combine(tempDirectory, package.Id.ToString());
                foreach (var action in package.Actions)
                {
                    ActionContext context = new ActionContext(tempSourceCopyDirectory, sourceContentPath, tempPackageDirectory);
                    action.Run(context);
                }

                // generate info required to validate
                var origTargetHashes = Utility.GetFileHashes(targetContentPath);
                var patchedTargetHashes = Utility.GetFileHashes(tempSourceCopyDirectory);

                // check for obvious failure
                if (patchedTargetHashes.Count != origTargetHashes.Count)
                    throw new InvalidDataException("Patch failed, output does not match the target.");

                // TODO: case-sensitivity
                //// compare target copy contents against the original
                //foreach (KeyValuePair<string, string> fileHashPair in origTargetHashes)
                //{
                //    if (!patchedTargetHashes.ContainsKey(fileHashPair.Key.ToLowerInvariant()))
                //        throw new FileNotFoundException(fileHashPair.Key + " not found in patch output.");

                //    var targetInfo = patchedTargetHashes[fileHashPair.Key.ToLowerInvariant()];

                //    if (fileHashPair.Value != targetInfo)
                //        throw new InvalidDataException(fileHashPair.Key + " does not match source file hash.");
                //}
            }
            finally
            {
                Directory.Delete(tempDirectory, true);
                Directory.Delete(tempSourceCopyDirectory, true);
            }
        }
    }
}
