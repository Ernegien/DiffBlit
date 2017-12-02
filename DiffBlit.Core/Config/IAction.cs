using DiffBlit.Core.Utilities;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// Describes a runnable action.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    [JsonConverter(typeof(ActionJsonConverter))]
    public interface IAction
    {
        /// <summary>
        /// Runs the action under the specified context.
        /// </summary>
        /// <param name="context">The action context.</param>
        void Run(ActionContext context);
    }
}
