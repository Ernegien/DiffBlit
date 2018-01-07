using DiffBlit.Core.Config;
using Newtonsoft.Json;
using DiffBlit.Core.IO;

namespace DiffBlit.Core.Actions
{
    /// <summary>
    /// Describes a runnable action.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    //[JsonConverter(typeof(ActionJsonConverter))]
    public interface IAction
    {
        /// <summary>
        /// The target path to run the action against.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        Path TargetPath { get; set; }

        /// <summary>
        /// Runs the action under the specified context.
        /// </summary>
        /// <param name="context">The action context.</param>
        void Run(ActionContext context);
    }
}
