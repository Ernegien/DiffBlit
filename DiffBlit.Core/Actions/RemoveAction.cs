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
    public class RemoveAction : IAction
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
        public Path TargetPath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonConstructor]
        private RemoveAction()
        {
            
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="targetPath"></param>
        public RemoveAction(string targetPath)
        {
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

            try
            {
                string path = Path.Combine(context.BasePath, TargetPath);
                if (TargetPath.IsDirectory)
                {
                    Logger.Info("Deleting directory {0}", path);
                    Directory.Delete(path);
                }
                else
                {
                    Logger.Info("Deleting file {0}", path);
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                // HACK: WPF apps load d3dcompiler_47 from current directory instead of from %windir%\system32\ preventing deletion
                if (TargetPath.ToString().EndsWith("d3dcompiler_47.dll", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Warn(ex, "Please disregard for now");
                    return;
                }

                throw;
            }
        }
    }
}
