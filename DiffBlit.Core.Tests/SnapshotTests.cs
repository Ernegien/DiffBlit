using System;
using DiffBlit.Core.Config;
using DiffBlit.Core.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiffBlit.Core.Tests
{
    [TestClass]
    public class SnapshotTests
    {
        static readonly Path SourceContentPath = System.IO.Path.Combine(Environment.CurrentDirectory, "content\\source");
        static readonly Path TargetContentPath = System.IO.Path.Combine(Environment.CurrentDirectory, "content\\target");
        private static readonly Snapshot sourceSnapshot = new Snapshot(SourceContentPath);
        private static readonly Snapshot sourceSnapshot2 = new Snapshot(SourceContentPath);
        private static readonly Snapshot targetSnapshot = new Snapshot(TargetContentPath);

        [TestMethod]
        public void SnapshotEqualityTest()
        {
            Assert.AreEqual(sourceSnapshot, sourceSnapshot2);
            Assert.AreNotEqual(sourceSnapshot, targetSnapshot);
        }
    }
}
