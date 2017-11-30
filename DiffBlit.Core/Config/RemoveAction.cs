using System.IO;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class RemoveAction : IAction
    {
        /// <summary>
        /// The type name used to aid in json deserialization.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        private const string Type = "remove";

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string TargetPath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        public RemoveAction()
        {
            
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        public RemoveAction(string targetPath)
        {
            TargetPath = targetPath;
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        public void Run()
        {
            File.Delete(TargetPath);
        }
    }
}
