using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DiffBlit.Core
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PatchAlgorithmType
    {
        BsDiff,
        Fossil,
        MsDelta,
        Octodiff,
        PatchApi,
        XDelta
    }
}
