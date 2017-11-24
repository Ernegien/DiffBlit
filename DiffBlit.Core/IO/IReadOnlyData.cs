namespace DiffBlit.Core.IO
{
    /// <summary>
    /// A generic interface for reading data.
    /// </summary>
    public interface IReadOnlyData
    {
        /// <summary>
        /// Opens a read-only stream to the data.
        /// </summary>
        ReadOnlyStream Open();
    }
}
