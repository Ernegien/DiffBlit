using System.Diagnostics;
using System.IO;
using DiffBlit.Core.Logging;
using Octodiff.Core;

namespace DiffBlit.Core.Delta
{
    /// <summary>
    /// TODO: description
    /// </summary>
    public class OctodiffPatcher : IPatcher
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
            Logger.Info("Creating Octodiff patch at {0} from {1} to {2}", deltaPath, sourcePath, targetPath);

            using (Stream orig = File.OpenRead(sourcePath))
            using (Stream target = File.OpenRead(targetPath))
            using (Stream patch = File.OpenWrite(deltaPath))
            using (MemoryStream sig = new MemoryStream())
            {
                new SignatureBuilder().Build(orig, new SignatureWriter(sig));
                sig.Position = 0;
                sig.Flush();

                DeltaBuilder b = new DeltaBuilder();
                b.BuildDelta(target, new SignatureReader(sig, b.ProgressReporter), new BinaryDeltaWriter(patch));
            }
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="deltaPath"></param>
        /// <param name="targetPath"></param>
        public void Apply(string sourcePath, string deltaPath, string targetPath)
        {
            Logger.Info("Applying Octodiff patch {0} against {1} to {2}", deltaPath, sourcePath, targetPath);

            using (var output = File.Open(targetPath, FileMode.Create, FileAccess.ReadWrite))
            using (var delta = File.OpenRead(deltaPath))
            using (var orig = File.OpenRead(sourcePath))
            {
                new DeltaApplier().Apply(orig, new BinaryDeltaReader(delta, null), output);
            }
        }
    }
}
