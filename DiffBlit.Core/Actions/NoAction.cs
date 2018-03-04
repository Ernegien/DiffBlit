using DiffBlit.Core.IO;
using Newtonsoft.Json;

namespace DiffBlit.Core.Actions
{
    /// <summary>
    /// This action does nothing when ran.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class NoAction : IAction
    {
        /// <summary>
        /// TODO: description
        /// </summary>
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
        private NoAction(bool optional = true)
        {
            Optional = optional;
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="targetPath"></param>
        public NoAction(Path targetPath)
        {
            TargetPath = targetPath;
        }

        public void Run(ActionContext context = null)
        {
            // do nothing
        }
    }
}
