using DeltaCompressionDotNet.PatchApi;

namespace DiffBlit.Core.Delta
{
    /// <summary>
    /// TODO: description
    /// </summary>
    public class PatchApiPatcher : IPatcher
    {
        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="deltaPath"></param>
        public void Create(string sourcePath, string targetPath, string deltaPath)
        {
            new PatchApiCompression().CreateDelta(sourcePath, targetPath, deltaPath);
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="deltaPath"></param>
        /// <param name="targetPath"></param>
        public void Apply(string sourcePath, string deltaPath, string targetPath)
        {
            new PatchApiCompression().ApplyDelta(deltaPath, sourcePath, targetPath);
        }
    }
}
