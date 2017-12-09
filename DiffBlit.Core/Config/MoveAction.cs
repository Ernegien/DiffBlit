using System;
using System.IO;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class MoveAction : IAction
    {
        /// <summary>
        /// The type name used to aid in json deserialization.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        private const ActionType Type = ActionType.Move;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public FilePath SourcePath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public FilePath TargetPath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        public MoveAction()
        {
            
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        public MoveAction(FilePath sourcePath, FilePath targetPath)
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

            FilePath targetPath = Path.Combine(context.BasePath, TargetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            File.Move(Path.Combine(context.BasePath, SourcePath), targetPath);
        }
    }
}
