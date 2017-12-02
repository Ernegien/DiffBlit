using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiffBlit.Core.Utilities
{
    /// <summary>
    /// Converts System.Version to/from a typical Major.Minor.Build.Revision string.
    /// </summary>
    public class VersionJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((Version)value).ToString());
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new Version((string)serializer.Deserialize<JValue>(reader));
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return typeof(Version).IsAssignableFrom(objectType);
        }
    }
}
