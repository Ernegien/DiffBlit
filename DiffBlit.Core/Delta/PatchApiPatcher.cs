using DeltaCompressionDotNet.PatchApi;

namespace DiffBlit.Core.Delta
{
    public class PatchApiPatcher : IPatcher
    {
        public void Create(string sourcePath, string targetPath, string deltaPath)
        {
            new PatchApiCompression().CreateDelta(sourcePath, targetPath, deltaPath);
        }

        public void Apply(string sourcePath, string deltaPath, string targetPath)
        {
            new PatchApiCompression().ApplyDelta(deltaPath, sourcePath, targetPath);
        }
    }
}
