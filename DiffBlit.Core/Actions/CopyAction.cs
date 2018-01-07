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
    public class CopyAction : IAction
    {
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

            Path targetPath = Path.Combine(context.BasePath, TargetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            File.Copy(Path.Combine(context.BasePath, SourcePath), targetPath);
        }
    }
}
