using DiffBlit.Core.IO;

namespace DiffBlit.Core.Actions
{
    /// <summary>
    /// Context for IAction execution.
    /// </summary>
    public class ActionContext
    {
        /// <summary>
        /// The local base path of the object(s) to take action against.
        /// </summary>
        public Path BasePath { get; set; }

        /// <summary>
        /// The base content path to use associated with the action.
        /// </summary>
        public Path ContentBasePath { get; set; }

        /// <summary>
        /// Initializes the action context.
        /// </summary>
        /// <param name="basePath">The optional base directory of the object(s) to take action against.</param>
        /// <param name="contentBasePath">The optional content directory to use associated with the action.</param>
        public ActionContext(Path basePath = null, Path contentBasePath = null)
        {
            BasePath = basePath;
            ContentBasePath = contentBasePath;
        }
    }
}
