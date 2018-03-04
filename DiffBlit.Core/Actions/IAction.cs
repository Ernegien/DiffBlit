using Newtonsoft.Json;
using DiffBlit.Core.IO;

namespace DiffBlit.Core.Actions
{
    /// <summary>
    /// Describes a runnable action.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public interface IAction
    {
        /// <summary>
        /// The target path to run the action against.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        Path TargetPath { get; set; }

        /// <summary>
        /// Throws an exception upon failure if false, otherwise indicates the action is not required for successful package application.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        bool Optional { get; set; }

        /// <summary>
        /// Runs the action under the specified context.
        /// </summary>
        /// <param name="context">The action context.</param>
        void Run(ActionContext context);
    }
}
