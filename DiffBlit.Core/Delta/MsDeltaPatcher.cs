using System.Diagnostics;
using DeltaCompressionDotNet.MsDelta;
using DiffBlit.Core.Logging;

namespace DiffBlit.Core.Delta
{
    /// <summary>
    /// TODO: description
    /// </summary>
    public class MsDeltaPatcher : IPatcher
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
            Logger.Info("Creating MsDelta patch at {0} from {1} to {2}", deltaPath, sourcePath, targetPath);
            new MsDeltaCompression().CreateDelta(sourcePath, targetPath, deltaPath);
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="deltaPath"></param>
        /// <param name="targetPath"></param>
        public void Apply(string sourcePath, string deltaPath, string targetPath)
        {
            Logger.Info("Applying MsDelta patch {0} against {1} to {2}", deltaPath, sourcePath, targetPath);
            new MsDeltaCompression().ApplyDelta(deltaPath, sourcePath, targetPath);
        }
    }
}
