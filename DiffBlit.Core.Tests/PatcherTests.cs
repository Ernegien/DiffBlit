using System.IO;
using DiffBlit.Core.Extensions;
using DiffBlit.Core.Delta;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiffBlit.Core.Tests
{
    [TestClass]
    public class PatcherTests
    {
        private static readonly string SourceFilePath = Path.GetTempFileName();
        private static readonly string TargetFilePath = Path.GetTempFileName();
        private static readonly string PatchFilePath = Path.GetTempFileName();
        private static readonly string PatchedFilePath = Path.GetTempFileName();

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            byte[] source = new byte[0x1000].FillRandom();
            for (int i = 0; i < source.Length / 2; i++)
            {
                source[i] = 0;
            }

            byte[] destination = new byte[0x2000].FillRandom();
            for (int i = 0; i < destination.Length / 2; i++)
            {
                destination[i] = 0;
            }

            File.WriteAllBytes(SourceFilePath, source);
            File.WriteAllBytes(TargetFilePath, destination);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(PatchFilePath);
            File.Delete(PatchedFilePath);
        }

        [TestMethod]
        public void BsDiffPatcherTest()
        {
            IPatcher patcher = new BsDiffPatcher();
            patcher.Create(SourceFilePath, TargetFilePath, PatchFilePath);
            patcher.Apply(SourceFilePath, PatchFilePath, PatchedFilePath);
            Assert.IsTrue(File.ReadAllBytes(TargetFilePath).IsEqual(File.ReadAllBytes(PatchedFilePath)));
        }

        [TestMethod]
        public void MsDeltaPatcherTest()
        {
            IPatcher patcher = new MsDeltaPatcher();
            patcher.Create(SourceFilePath, TargetFilePath, PatchFilePath);
            patcher.Apply(SourceFilePath, PatchFilePath, PatchedFilePath);
            Assert.IsTrue(File.ReadAllBytes(TargetFilePath).IsEqual(File.ReadAllBytes(PatchedFilePath)));
        }

        [TestMethod]
        public void PatchApiPatcherTest()
        {
            IPatcher patcher = new PatchApiPatcher();
            patcher.Create(SourceFilePath, TargetFilePath, PatchFilePath);
            patcher.Apply(SourceFilePath, PatchFilePath, PatchedFilePath);
            Assert.IsTrue(File.ReadAllBytes(TargetFilePath).IsEqual(File.ReadAllBytes(PatchedFilePath)));
        }

        [TestMethod]
        public void XDeltaPatcherTest()
        {
            IPatcher patcher = new XDeltaPatcher();
            patcher.Create(SourceFilePath, TargetFilePath, PatchFilePath);
            patcher.Apply(SourceFilePath, PatchFilePath, PatchedFilePath);
            Assert.IsTrue(File.ReadAllBytes(TargetFilePath).IsEqual(File.ReadAllBytes(PatchedFilePath)));
        }

        [TestMethod]
        public void FossilDeltaPatcherTest()
        {
            IPatcher patcher = new FossilPatcher();
            patcher.Create(SourceFilePath, TargetFilePath, PatchFilePath);
            patcher.Apply(SourceFilePath, PatchFilePath, PatchedFilePath);
            Assert.IsTrue(File.ReadAllBytes(TargetFilePath).IsEqual(File.ReadAllBytes(PatchedFilePath)));
        }

        [TestMethod]
        public void OctodiffPatcherTest()
        {
            IPatcher patcher = new OctodiffPatcher();
            patcher.Create(SourceFilePath, TargetFilePath, PatchFilePath);
            patcher.Apply(SourceFilePath, PatchFilePath, PatchedFilePath);
            Assert.IsTrue(File.ReadAllBytes(TargetFilePath).IsEqual(File.ReadAllBytes(PatchedFilePath)));
        }
    }
}
