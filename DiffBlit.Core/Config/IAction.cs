using DiffBlit.Core.Utilities;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    [JsonConverter(typeof(ActionJsonConverter))]
    public interface IAction
    {
        /// <summary>
        /// TODO: description
        /// </summary>
        void Run();
    }
}
