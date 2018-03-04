using System;
using System.Diagnostics;
using System.IO;
using DiffBlit.Core.Delta;
using DiffBlit.Core.Logging;
using DiffBlit.Core.Utilities;
using Newtonsoft.Json;
using Path = DiffBlit.Core.IO.Path;

namespace DiffBlit.Core.Actions
{
    // TODO: better support for remote paths which will optionally require credentials to be specified in the context

    /// <summary>
    /// Patches the source file against patch content saving to the target path.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class PatchAction : IAction
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
        /// The target file path.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Path TargetPath { get; set; }

        /// <summary>
        /// The patch algorithm used.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public PatchAlgorithmType Algorithm { get; set; }

        /// <summary>
        /// The patch content.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Content Content { get; private set; }

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
        private PatchAction()
        {
            // required for serialization
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="algorithm"></param>
        /// <param name="content"></param>
        /// <param name="overwrite"></param>
        /// <param name="optional"></param>
        public PatchAction(Path sourcePath, Path targetPath, PatchAlgorithmType algorithm, Content content, bool overwrite = true, bool optional = false)
        {
            SourcePath = sourcePath ?? throw new ArgumentException(nameof(sourcePath));
            TargetPath = targetPath ?? throw new ArgumentException(nameof(targetPath));
            Algorithm = algorithm;
            Content = content ?? throw new ArgumentException(nameof(content));
            Overwrite = overwrite;
            Optional = optional;
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="context"></param>
        public void Run(ActionContext context)
        {
            if (!SourcePath.IsAbsolute && context?.BasePath == null)
                throw new ArgumentException("The context BasePath must be specified when the SourcePath is relative.", nameof(context));

            if (!TargetPath.IsAbsolute && context?.BasePath == null)
                throw new ArgumentException("The context BasePath must be specified when the TargetPath is relative.", nameof(context));

            if (Content != null && context?.ContentBasePath == null)
                throw new ArgumentException("The context ContentBasePath must be specified when working with Content.", nameof(context));

            if (SourcePath.IsDirectory || TargetPath.IsDirectory)
                throw new NotSupportedException("Both the source and target paths must be files.");

            string tempPatchPath = Utility.GetTempFilePath();
            string tempTargetCopyPath = Utility.GetTempFilePath();

            try
            {
                // get the absolute paths, rooted off of the context base path if necessary
                Path sourcePath = SourcePath.IsAbsolute ? SourcePath : Path.Combine(context.BasePath, SourcePath);
                Path targetPath = TargetPath.IsAbsolute ? TargetPath : Path.Combine(context.BasePath, TargetPath);

                IPatcher patcher = Utility.GetPatcher(Algorithm);

                // write the patch file to a temp location
                Content.Save(Path.Combine(context.ContentBasePath, Content.Id + "\\"), tempPatchPath,
                    Overwrite); // TODO: specify remote content path format as part of context

                // if source and target paths are the same, copy source to temp location first to patch against
                if (SourcePath.Equals(TargetPath))
                {
                    File.Copy(sourcePath, tempTargetCopyPath); // TODO: support for remote paths
                    patcher.Apply(tempTargetCopyPath, tempPatchPath, targetPath); // TODO: support for remote paths
                }
                else
                {
                    patcher.Apply(sourcePath, tempPatchPath, targetPath); // TODO: support for remote paths
                }
            }
            catch
            {
                // swallow the exception if optional
                if (!Optional)
                    throw;
            }
            finally
            {
                File.Delete(tempPatchPath);
                File.Delete(tempTargetCopyPath);
            }
        }
    }
}
