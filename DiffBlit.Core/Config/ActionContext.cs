namespace DiffBlit.Core.Config
{
    /// <summary>
    /// Context for IAction execution.
    /// </summary>
    public class ActionContext
    {
        /// <summary>
        /// The source directory to use associated with the action.
        /// </summary>
        public string SourceBasePath { get; set; }

        /// <summary>
        /// The target directory of the object to take action against.
        /// </summary>
        public string TargetBasePath { get; set; }

        /// <summary>
        /// The content directory to use associated with the action.
        /// </summary>
        public string ContentBasePath { get; set; }

        /// <summary>
        /// Initializes the action context.
        /// </summary>
        public ActionContext()
        {
            
        }

        /// <summary>
        /// Initializes the action context.
        /// </summary>
        /// <param name="targetBasePath">The target directory of the object to take action against.</param>
        /// <param name="sourceBasePath">The optional source directory to use associated with the action.</param>
        /// <param name="contentBasePath">The optional content directory to use associated with the action.</param>
        public ActionContext(string targetBasePath, string sourceBasePath = null, string contentBasePath = null)
        {
            TargetBasePath = targetBasePath;
            SourceBasePath = sourceBasePath;
            ContentBasePath = contentBasePath;
        }
    }
}
