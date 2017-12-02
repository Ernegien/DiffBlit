using System;
using System.IO;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class CopyAction : IAction
    {
        /// <summary>
        /// The type name used to aid in json deserialization.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        private const ActionType Type = ActionType.Copy;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string SourcePath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string TargetPath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        public CopyAction()
        {

        }

        /// <summary>
        /// TODO: description
        /// </summary>
        public CopyAction(string sourcePath, string targetPath)
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

            if (context.SourceBasePath == null)
                throw new NullReferenceException("Source base path must be specified.");

            if (context.TargetBasePath == null)
                throw new NullReferenceException("Target base path must be specified.");

            string targetPath = Path.Combine(context.TargetBasePath, TargetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            File.Copy(Path.Combine(context.SourceBasePath, SourcePath), targetPath);
        }
    }
}
