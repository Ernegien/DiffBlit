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
    /// Removes the target file or directory.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class RemoveAction : IAction
    {
        /// <summary>
        /// The current logging instance which may be null until defined by the caller.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ILogger Logger => LoggerBase.CurrentInstance;

        /// <summary>
        /// The target file or directory path.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Path TargetPath { get; set; }

        /// <summary>
        /// Throws an exception upon failure if false, otherwise indicates the action is not required for successful package application.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool Optional { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonConstructor]
        private RemoveAction()
        {
            // required for serialization
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="optional"></param>
        public RemoveAction(Path targetPath, bool optional = false)
        {
            TargetPath = targetPath ?? throw new ArgumentException(nameof(targetPath));
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

            try
            {
                // get the absolute path, rooted off of the context base path if necessary
                Path path = TargetPath.IsAbsolute ? TargetPath : Path.Combine(context.BasePath, TargetPath);
                Logger.Info("Deleting {0}", path);

                if (TargetPath.IsDirectory)
                {
                    Directory.Delete(path, true);   // TODO: support for remote paths
                }
                else
                {
                    File.Delete(path);   // TODO: support for remote paths
                }
            }
            catch (Exception ex)
            {
                // swallow the exception and log a warning if optional, otherwise propagate upwards
                if (Optional)
                {
                    Logger.Warn(ex, "Optional action failure.");
                }
                else throw;
            }
        }
    }
}
