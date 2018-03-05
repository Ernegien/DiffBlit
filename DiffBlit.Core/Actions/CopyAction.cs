using System;
using System.Diagnostics;
using System.IO;
using DiffBlit.Core.Logging;
using Newtonsoft.Json;
using Path = DiffBlit.Core.IO.Path;

namespace DiffBlit.Core.Actions
{
    // TODO: better support for remote paths which will optionally require credentials to be specified in the context

    /// <summary>
    /// Copies the source file to the target path.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class CopyAction : IAction
    {
        /// <summary>
        /// The current logging instance which may be null until defined by the caller.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ILogger Logger => LoggerBase.CurrentInstance;

        /// <summary>
        /// The source file path.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Path SourcePath { get; set; }

        /// <summary>
        /// The local target file path.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Path TargetPath { get; set; }

        /// <summary>
        /// Allows overwriting the target file if true.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool Overwrite { get; set; }

        /// <summary>
        /// Throws an exception upon failure if false, otherwise indicates the action is not required for successful package application.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool Optional { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonConstructor]
        private CopyAction()
        {
            // required for serialization
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="overwrite"></param>
        /// <param name="optional"></param>
        public CopyAction(Path sourcePath, Path targetPath, bool overwrite = false, bool optional = false)
        {
            SourcePath = sourcePath ?? throw new ArgumentException(nameof(sourcePath));
            TargetPath = targetPath ?? throw new ArgumentException(nameof(targetPath));
            Overwrite = overwrite;
            Optional = optional;
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="context"></param>
        public void Run(ActionContext context = null)
        {
            if (!SourcePath.IsAbsolute && context?.BasePath == null)
                throw new ArgumentException("The context BasePath must be specified when the SourcePath is relative.", nameof(context));

            if (!TargetPath.IsAbsolute && context?.BasePath == null)
                throw new ArgumentException("The context BasePath must be specified when the TargetPath is relative.", nameof(context));

            if (SourcePath.Equals(TargetPath))
                throw new NotSupportedException("The SourcePath cannot equal the TargetPath.");

            if (SourcePath.IsDirectory || TargetPath.IsDirectory)
                throw new NotSupportedException("The source and target paths must both be files.");

            try
            {
                // get the absolute file paths, rooted off of the context base path if necessary
                Path sourcePath = SourcePath.IsAbsolute ? SourcePath : Path.Combine(context.BasePath, SourcePath);
                Path targetPath = TargetPath.IsAbsolute ? TargetPath : Path.Combine(context.BasePath, TargetPath);

                // ensure the target directory path exists
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));   // TODO: support for remote paths

                Logger?.Info("Copying {0} to {1}", sourcePath, targetPath);
                File.Copy(sourcePath, targetPath, Overwrite);   // TODO: support for remote paths
            }
            catch (Exception ex)
            {
                // swallow the exception and log a warning if optional, otherwise propagate upwards
                if (Optional)
                {
                    Logger?.Warn(ex, "Optional action failure.");
                }
                else throw;
            }
        }
    }
}
