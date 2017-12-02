using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using DiffBlit.Core.Extensions;

namespace DiffBlit.Core.Delta
{
    // TODO: find C# port or compile library to call instead of having to invoke an exe :X

    /// <summary>
    /// TODO: description
    /// </summary>
    public class XDeltaPatcher : IPatcher
    {
        private static string BinaryName = "xdelta3.exe";
        private static readonly string TempDirectory = Path.GetTempPath();
        private static readonly string BinaryPath = Path.Combine(TempDirectory, BinaryName);

        /// <summary>
        /// TODO: description
        /// </summary>
        public XDeltaPatcher()
        {
            Assembly.GetExecutingAssembly().ExtractEmbeddedResource(TempDirectory, "DiffBlit.Core.Resources", "xdelta3.exe");
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="deltaPath"></param>
        public void Create(string sourcePath, string targetPath, string deltaPath)
        {
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
