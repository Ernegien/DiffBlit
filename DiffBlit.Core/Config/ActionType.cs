using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// Package action type used to aid in json deserialization.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActionType
    {
        Add,
        Remove,
        Move,
        Copy,
        Patch
    }
}
