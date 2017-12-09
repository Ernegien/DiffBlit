namespace DiffBlit.Core.Config
{
    /// <summary>
    /// Context for IAction execution.
    /// </summary>
    public class ActionContext
    {
        /// <summary>
        /// The base directory of the object(s) to take action against.
        /// </summary>
        public FilePath BasePath { get; set; }

        /// <summary>
        /// The content directory to use associated with the action.
        /// </summary>
        public FilePath ContentBasePath { get; set; }

        /// <summary>
        /// Initializes the action context.
        /// </summary>
        public ActionContext()
        {
            
        }

        /// <summary>
        /// Initializes the action context.
        /// </summary>
        /// <param name="basePath">The base directory of the object(s) to take action against.</param>
        /// <param name="contentBasePath">The optional content directory to use associated with the action.</param>
        public ActionContext(FilePath basePath, FilePath contentBasePath = null)
        {
            BasePath = basePath;
            ContentBasePath = contentBasePath;
        }
    }
}
