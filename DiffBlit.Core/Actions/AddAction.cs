using System.IO;
using DiffBlit.Core.Config;
using Newtonsoft.Json;
using Path = DiffBlit.Core.IO.Path;

namespace DiffBlit.Core.Actions
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class AddAction : IAction
    {
        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Path TargetPath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public Content Content { get; private set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonConstructor]
        private AddAction()
        {
            
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        public AddAction(Path targetPath, Content content)
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
            Path path = Path.Combine(context.BasePath, TargetPath);
            if (path.IsDirectory)
            {
                Directory.CreateDirectory(path);
            }
            else if (Content != null)
            {
                Content.Save(Path.Combine(context.ContentBasePath, Content.Id + "\\"), path);
            }
            else File.Create(path).Dispose();
        }
    }
}
