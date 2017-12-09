using System;
using DiffBlit.Core.Utilities;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    [JsonConverter(typeof(ToStringJsonConverter))]
    public class FilePath : IEquatable<FilePath>
    {
        public bool CaseSensitive { get; }

        public string Path { get; }

        public bool IsDirectory => Path.EndsWith("/") || Path.EndsWith("\\");

        public bool IsAbsolute { get; }

        public FilePath(string path, bool caseSensitive = false)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri uri))
                throw new ArgumentException();

            // TODO: tighten up what constitutes as valid; for now, don't be a dick

            IsAbsolute = uri.IsAbsoluteUri;
            Path = path;
            CaseSensitive = caseSensitive;
        }

        /// <summary>
        /// Implicitly attempts to convert a string to a file path.
        /// </summary>
        /// <param name="path"></param>
        public static implicit operator FilePath (string path)
        {
            return new FilePath(path);
        }

        /// <summary>
        /// Implicitly converts a file path to a string.
        /// </summary>
        /// <param name="path"></param>
        public static implicit operator string (FilePath path)
        {
            return path.Path;
        }

        public bool Equals(FilePath other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Path, other.Path, CaseSensitive || other.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(string other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Path, other, CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FilePath) obj);
        }

        public override int GetHashCode()
        {
            if (Path == null)
                return 0;

            return CaseSensitive ? Path.GetHashCode() : Path.ToUpperInvariant().GetHashCode();
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
