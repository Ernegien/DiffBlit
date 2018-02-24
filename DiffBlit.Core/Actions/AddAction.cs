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
    public class AddAction : IAction
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
        [JsonProperty(Required = Required.Default)]
        public Content Content { get; private set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonConstructor]
        private AddAction()
        {
            
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        public AddAction(Path targetPath, Content content)
        {
            TargetPath = targetPath;
            Content = content;
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="context"></param>
        public void Run(ActionContext context)
        {
            try
            {
                Path path = Path.Combine(context.BasePath, TargetPath);
                if (path.IsDirectory)
                {
                    Logger.Info("Creating directory {0}", path);
                    Directory.CreateDirectory(path);
                }
                else 
                {
                    Logger.Info("Creating file {0}", path);

                    if (Content != null)
                    {
                        Content.Save(Path.Combine(context.ContentBasePath, Content.Id + "\\"), path);
                    }
                    else File.Create(path).Dispose();
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
