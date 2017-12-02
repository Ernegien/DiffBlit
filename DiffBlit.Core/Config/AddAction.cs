using System;
using System.IO;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class AddAction : IAction
    {
        /// <summary>
        /// The type name used to aid in json deserialization.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        private const ActionType Type = ActionType.Add;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string TargetPath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Content Content { get; } = new Content();

        /// <summary>
        /// TODO: description
        /// </summary>
        public AddAction()
        {
            
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        public AddAction(string targetPath, Content content)
        {
            TargetPath = targetPath;
            Content = content;
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="context"></param>
        public void Run(ActionContext context)
        {
            Content.Save(Path.Combine(context.ContentBasePath, Content.Id.ToString()), Path.Combine(context.TargetBasePath, TargetPath));
        }
    }
}
