using System;
using System.Diagnostics;
using System.IO;
using DiffBlit.Core.Logging;
using Newtonsoft.Json;
using Path = DiffBlit.Core.IO.Path;

namespace DiffBlit.Core.Actions
{
    // TODO: better support for remote paths which will optionally require credentials to be specified in the context
    // TODO: copy to target and delete from source when working with remote paths

    /// <summary>
    /// Moves the source file or directory to the target path.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class MoveAction : IAction
    {
        /// <summary>
        /// The current logging instance which may be null until defined by the caller.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ILogger Logger => LoggerBase.CurrentInstance;

        /// <summary>
        /// The source file or directory path.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Path SourcePath { get; set; }

        /// <summary>
        /// The target file or directory path.
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
        private MoveAction()
        {
            // required for serialization
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        public MoveAction(Path sourcePath, Path targetPath, bool overwrite = false, bool optional = false)
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

            if (SourcePath.IsDirectory != TargetPath.IsDirectory)
                throw new NotSupportedException("Both the source and target paths must be of the same type.");

            try
            {
                // get the absolute paths, rooted off of the context base path if necessary
                Path sourcePath = SourcePath.IsAbsolute ? SourcePath : Path.Combine(context.BasePath, SourcePath);
                Path targetPath = TargetPath.IsAbsolute ? TargetPath : Path.Combine(context.BasePath, TargetPath);

                // ensure that target directory path exists
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));   // TODO: support for remote paths

                Logger.Info("Moving {0} to {1}", sourcePath, targetPath);
                if (SourcePath.IsDirectory)
                {
                    Directory.Move(sourcePath, targetPath);   // TODO: support for remote paths
                }
                else
                {
                    File.Move(sourcePath, targetPath);   // TODO: support for remote paths
                }
            }
            catch
            {
                // swallow the exception if optional
                if (!Optional)
                    throw;
            }
        }
    }
}
