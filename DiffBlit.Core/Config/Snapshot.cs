using System;
using System.Collections.Generic;
using System.IO;
using DiffBlit.Core.Utilities;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Snapshot : IEquatable<Snapshot>
    {
        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public DateTime DateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        [JsonConverter(typeof(VersionJsonConverter))]
        public Version Version { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public Dictionary<string, object> Attributes { get; } = new Dictionary<string, object>();

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Content Content { get; } = new Content();

        /// <summary>
        /// Generates a snapshot using the specified path information.
        /// </summary>
        /// <param name="path">The content path.</param>
        /// <returns>The created snapshot information.</returns>
        public static Snapshot Create(string path)
        {
            Snapshot snapshot = new Snapshot();

            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                var hash = Utility.ComputeHash(file);
                var relativePath = file.Substring(path.Length + 1); // file path relative to the base content path specified
                snapshot.Content.Add(new FileInformation(relativePath, hash));
            }

            return snapshot;
        }

        public bool Equals(Snapshot other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Content, other.Content);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Snapshot) obj);
        }

        public override int GetHashCode()
        {
            return (Content != null ? Content.GetHashCode() : 0);
        }
    }
}
