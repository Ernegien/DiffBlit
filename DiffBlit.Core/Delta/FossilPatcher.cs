using System.IO;
using DiffBlit.Core.Extensions;
using Ionic.BZip2;

namespace DiffBlit.Core.Delta
{
    public class FossilPatcher : IPatcher
    {
        public void Create(string sourcePath, string targetPath, string deltaPath)
        {
            using (FileStream patch = File.Open(deltaPath, FileMode.Create, FileAccess.Write))
            using (BZip2OutputStream zip = new BZip2OutputStream(patch))
            {
                byte[] patchData = Fossil.Delta.Create(File.ReadAllBytes(sourcePath), File.ReadAllBytes(targetPath));
                zip.Write(patchData, 0, patchData.Length);
            }
        }

        public void Apply(string sourcePath, string deltaPath, string targetPath)
        {
            using (BZip2InputStream patch = new BZip2InputStream(File.OpenRead(deltaPath)))
            using (Stream output = File.Open(targetPath, FileMode.Create, FileAccess.Write))
            {
                byte[] patchData = patch.ReadAllBytes();
                byte[] patchedData = Fossil.Delta.Apply(File.ReadAllBytes(sourcePath), patchData);
                output.Write(patchedData, 0, patchedData.Length);
            }
        }
    }
}
