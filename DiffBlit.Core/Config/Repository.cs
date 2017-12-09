using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Repository
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
        [JsonProperty(Required = Required.Default)]
        public Dictionary<string, object> Attributes { get; } = new Dictionary<string, object>();

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public List<Snapshot> Snapshots { get; } = new List<Snapshot>();

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public List<Package> Packages { get; } = new List<Package>();

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Repository Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<Repository>(json);
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        
        /// <summary>
        /// Returns the matching snapshot contained in the directory, otherwise null.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public Snapshot FindSnapshotFromDirectory(FilePath directory)
        {
            // searches snapshots by file count in descending order to find the most specific match possible
            return Snapshots.OrderByDescending(s => s.Files.Count).FirstOrDefault(snapshot => new Snapshot(directory).Contains(snapshot));
        }
    }
}