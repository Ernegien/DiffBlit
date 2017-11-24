using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Package
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
        public DateTime DateTime { get; } = DateTime.UtcNow;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Guid SourceSnapshotId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Guid TargetSnapshotId { get; set; } = Guid.NewGuid();

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
        public IList<IAction> Actions { get; } = new List<IAction>();

        /// <summary>
        /// Generates package contents in the specified output path and returns the updated repository configuration.
        /// </summary>
        /// <param name="repo">The initial repository configuration.</param>
        /// <param name="sourcePath">The source content path.</param>
        /// <param name="targetPath">The target content path.</param>
        /// <param name="outputPath">The package content path.</param>
        /// <returns></returns>
        public static Repository Create(Repository repo, string sourcePath, string targetPath, string outputPath)
        {
            // TODO: generate snapshots for both source and target if they don't already exist (don't include updater app)

            // TODO: determine actions required based on differences between the two

            // TODO: generate patches where necessary and save in output path

            // TODO: return updated repo config
            return null;
        }
    }
}
