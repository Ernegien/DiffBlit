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
        public void Run()
        {
            File.Copy(SourcePath, TargetPath);
        }
    }
}
