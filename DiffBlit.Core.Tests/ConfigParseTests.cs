using System.IO;
using DiffBlit.Core.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiffBlit.Core.Tests
{
    [TestClass]
    public class ConfigParseTests
    {
        [TestMethod]
        public void SimpleSerializationTest()
        {
            var repo = Repository.Deserialize(File.ReadAllText("content\\repo.json"));
            string json = repo.Serialize();
        }
    }
}
