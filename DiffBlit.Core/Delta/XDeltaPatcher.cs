using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using DiffBlit.Core.Extensions;
using DiffBlit.Core.Logging;

namespace DiffBlit.Core.Delta
{
    // TODO: find C# port or compile library to call instead of having to invoke an exe :X

    /// <summary>
    /// TODO: description
    /// </summary>
    public class XDeltaPatcher : IPatcher
    {
        /// <summary>
        /// The current logging instance which may be null until defined by the caller.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ILogger Logger => LoggerBase.CurrentInstance;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly string BinaryPath = Path.Combine(Path.GetTempPath(), "xdelta3.exe");

        /// <summary>
        /// TODO: description
        /// </summary>
        public XDeltaPatcher()
        {
            try
            {
                if (!File.Exists(BinaryPath))
                {
                    Logger.Info("Extracting XDelta resource");
                    Assembly.GetExecutingAssembly().ExtractEmbeddedResource("DiffBlit.Core.Resources.xdelta3.exe", BinaryPath);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "XDelta resource extraction failed");
            }
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="deltaPath"></param>
        public void Create(string sourcePath, string targetPath, string deltaPath)
        {
            Logger.Info("Creating XDelta patch at {0} from {1} to {2}", deltaPath, sourcePath, targetPath);

            ProcessStartInfo info = new ProcessStartInfo(BinaryPath, 
                $"-e -f -A= -s \"{sourcePath}\" \"{targetPath}\" \"{deltaPath}\"");
            info.CreateNoWindow = true;
            info.UseShellExecute = false;

            var p = Process.Start(info);
            p?.WaitForExit();
            
            if (p?.ExitCode != 0)
            {
                throw new Exception();
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
            Logger.Info("Applying XDelta patch {0} against {1} to {2}", deltaPath, sourcePath, targetPath);

            ProcessStartInfo info = new ProcessStartInfo(BinaryPath,
                $"-d -f -s \"{sourcePath}\" \"{deltaPath}\" \"{targetPath}\"");
            info.CreateNoWindow = true;
            info.UseShellExecute = false;

            var p = Process.Start(info);
            p?.WaitForExit();

            if (p?.ExitCode != 0)
            {
                throw new Exception();
            }
        }
    }
}
