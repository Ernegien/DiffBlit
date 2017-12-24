using System;
using Newtonsoft.Json;

namespace DiffBlit.Core.Utilities
{
    /// <summary>
    /// Serializes an object to its string representation.
    /// </summary>
    public class ToStringJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();    // TODO: 
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
