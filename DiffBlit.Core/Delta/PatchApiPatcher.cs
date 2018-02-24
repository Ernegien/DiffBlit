using System.Diagnostics;
using DeltaCompressionDotNet.PatchApi;
using DiffBlit.Core.Logging;

namespace DiffBlit.Core.Delta
{
    /// <summary>
    /// TODO: description
    /// </summary>
    public class PatchApiPatcher : IPatcher
    {
        /// <summary>
        /// The current logging instance which may be null until defined by the caller.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ILogger Logger => LoggerBase.CurrentInstance;

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="deltaPath"></param>
        public void Create(string sourcePath, string targetPath, string deltaPath)
        {
            Logger.Info("Creating PatchApi patch at {0} from {1} to {2}", deltaPath, sourcePath, targetPath);
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
            Logger.Info("Applying PatchApi patch {0} against {1} to {2}", deltaPath, sourcePath, targetPath);
            new PatchApiCompression().ApplyDelta(deltaPath, sourcePath, targetPath);
        }
    }
}
