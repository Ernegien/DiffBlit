using DeltaCompressionDotNet.MsDelta;

namespace DiffBlit.Core.Delta
{
    public class MsDeltaPatcher : IPatcher
    {
        public void Create(string sourcePath, string targetPath, string deltaPath)
        {
            new MsDeltaCompression().CreateDelta(sourcePath, targetPath, deltaPath);
        }

        public void Apply(string sourcePath, string deltaPath, string targetPath)
        {
            new MsDeltaCompression().ApplyDelta(deltaPath, sourcePath, targetPath);
        }
    }
}
