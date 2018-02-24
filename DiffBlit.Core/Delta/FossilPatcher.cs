using System.Diagnostics;
using System.IO;
using DiffBlit.Core.Extensions;
using DiffBlit.Core.Logging;
using Ionic.BZip2;

namespace DiffBlit.Core.Delta
{
    /// <summary>
    /// TODO: description
    /// </summary>
    public class FossilPatcher : IPatcher
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
            Logger.Info("Creating Fossil patch at {0} from {1} to {2}", deltaPath, sourcePath, targetPath);
            using (FileStream patch = File.Open(deltaPath, FileMode.Create, FileAccess.Write))
            using (BZip2OutputStream zip = new BZip2OutputStream(patch))
            {
                byte[] patchData = Fossil.Delta.Create(File.ReadAllBytes(sourcePath), File.ReadAllBytes(targetPath));
                zip.Write(patchData, 0, patchData.Length);
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
            Logger.Info("Applying Fossil patch {0} against {1} to {2}", deltaPath, sourcePath, targetPath);
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
