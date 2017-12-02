namespace DiffBlit.Core.Delta
{
    // TODO: estimate methods for create/apply memory usage?
    // TODO: work with streams instead, although this will be more kludgy as most patchers don't expose this

    /// <summary>
    /// TODO: description
    /// </summary>
    public interface IPatcher
    {
        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="deltaPath"></param>
        void Create(string sourcePath, string targetPath, string deltaPath);

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="deltaPath"></param>
        /// <param name="targetPath"></param>
        void Apply(string sourcePath, string deltaPath, string targetPath);
    }
}
