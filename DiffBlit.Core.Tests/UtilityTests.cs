using System.IO;
using DiffBlit.Core.Extensions;
using DiffBlit.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiffBlit.Core.Tests
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void HashTest()
        {
            Assert.AreEqual(Utility.ComputeHash("content\\source\\1\\empty1.txt").ToBase64(),
                "N8eDuAsdRYuJ5xLC3+J3cFDv8K78n22L7t7neAfZrrLifRSBXPTwIpsdNsGGu18rXvVeYysQjMQen7lkw5tCpQ==");

            Assert.AreEqual(Utility.ComputeHash("content\\source\\2\\duplicate.txt").ToBase64(),
                "IcuZfgduZ1+VR5dvj1Jj26gbuYo/ypQ6WH50OCupbPioAHzRvQXixXuvc4X3Uy4bnhtz5iyT7iENSwtE490//w==");
        }

        [TestMethod]
        public void CompressionTest()
        {
            var origFilePath = Path.GetTempFileName();
            var compressedFilePath = Path.GetTempFileName();
            var decompressedFilePath = Path.GetTempFileName();

            try
            {
                File.WriteAllBytes(origFilePath, new byte[0x1000]);
                Utility.Compress(origFilePath, compressedFilePath);
                Utility.Decompress(compressedFilePath, decompressedFilePath);
                Assert.AreEqual(Utility.ComputeHash(origFilePath).ToBase64(), Utility.ComputeHash(decompressedFilePath).ToBase64());
            }
            finally
            {
                File.Delete(origFilePath);
                File.Delete(compressedFilePath);
                File.Delete(decompressedFilePath);
            }
        }
    }
}
