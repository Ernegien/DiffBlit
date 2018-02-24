using DiffBlit.Core.IO;
using Newtonsoft.Json;

namespace DiffBlit.Core.Actions
{
    /// <summary>
    /// This action does nothing when ran.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class NoAction : IAction
    {
        /// <summary>
        /// TODO: description
        /// </summary>
        public Path TargetPath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonConstructor]
        private NoAction()
        {
            
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="targetPath"></param>
        public NoAction(Path targetPath)
        {
            TargetPath = targetPath;
        }

        public void Run(ActionContext context)
        {
            // do nothing
        }
    }
}
