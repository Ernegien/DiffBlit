using System;
using System.Diagnostics;
using System.IO;
using DiffBlit.Core.Logging;
using Newtonsoft.Json;
using Path = DiffBlit.Core.IO.Path;

namespace DiffBlit.Core.Actions
{
    /// <summary>
    /// TODO: description
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
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Path SourcePath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Path TargetPath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonConstructor]
        private CopyAction()
        {

        }

        /// <summary>
        /// TODO: description
        /// </summary>
        public CopyAction(Path sourcePath, Path targetPath)
        {
            SourcePath = sourcePath;
            TargetPath = targetPath;
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="context"></param>
        public void Run(ActionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.BasePath == null)
                throw new NullReferenceException("The base path must be specified.");

            if (SourcePath.Equals(TargetPath))
                throw new NotSupportedException("Are you sure about that?");

            Path sourcePath = Path.Combine(context.BasePath, SourcePath);
            Path targetPath = Path.Combine(context.BasePath, TargetPath);

            // ensure the target directory path exists
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            Logger.Info("Copying {0} to {1}", sourcePath, targetPath);
            File.Copy(sourcePath, targetPath);
        }
    }
}
