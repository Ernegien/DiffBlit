namespace DiffBlit.Core.Delta
{
    // TODO: estimate methods for create/apply memory usage?
    // TODO: work with streams instead, although this will be more kludgy as most patchers don't expose this
    public interface IPatcher
    {
        void Create(string sourcePath, string targetPath, string deltaPath);

        void Apply(string sourcePath, string deltaPath, string targetPath);
    }
}
