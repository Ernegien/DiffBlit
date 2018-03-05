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
    /// Adds a file or directory to the specified target path.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class AddAction : IAction
    {
        /// <summary>
        /// The current logging instance which may be null until defined by the caller.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ILogger Logger => LoggerBase.CurrentInstance;

        /// <summary>
        /// The local target path.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Path TargetPath { get; set; }

        /// <summary>
        /// The optional source file content.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public Content Content { get; set; }

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
        private AddAction()
        {
            // required for serialization
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="content"></param>
        /// <param name="overwrite"></param>
        /// <param name="optional"></param>
        public AddAction(Path targetPath, Content content = null, bool overwrite = false, bool optional = false)
        {
            TargetPath = targetPath ?? throw new ArgumentNullException(nameof(targetPath));
            Content = content;
            Overwrite = overwrite;
            Optional = optional;
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="context"></param>
        public void Run(ActionContext context = null)
        {
            if (!TargetPath.IsAbsolute && context?.BasePath == null)
                throw new ArgumentException("The context BasePath must be specified when the TargetPath is relative.", nameof(context));

            if (Content != null && context?.ContentBasePath == null)
                throw new ArgumentException("The context ContentBasePath must be specified when working with Content.", nameof(context));

            try
            {
                // get the absolute path, rooted off of the context base path if necessary
                Path path = TargetPath.IsAbsolute ? TargetPath : Path.Combine(context.BasePath, TargetPath);
                Logger?.Info("Adding {0}", path);

                if (path.IsDirectory)
                {
                    Directory.CreateDirectory(path);   // TODO: support for remote paths
                }
                else if (File.Exists(path) && !Overwrite)
                {
                    throw new IOException("File already exists and cannot be overwritten.");
                }
                else if (Content != null)
                {
                    // TODO: remove trailing slash requirement for caller, handle that internally
                    Content.Save(Path.Combine(context.ContentBasePath, Content.Id + "\\"), path, Overwrite);    // TODO: specify remote content path format as part of context
                }
                else
                {
                    // create an empty file
                    File.Create(path).Dispose();   // TODO: support for remote paths
                }
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
