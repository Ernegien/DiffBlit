using System;
using System.IO;
using DiffBlit.Core.Config;
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

            string path = Path.Combine(context.BasePath, TargetPath);
            if (TargetPath.IsDirectory)
            {
                Directory.Delete(path);
            }
            else
            {
                File.Delete(path);
            }
        }
    }
}
