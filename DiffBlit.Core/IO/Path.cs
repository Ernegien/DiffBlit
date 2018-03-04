using System;
using Newtonsoft.Json;

namespace DiffBlit.Core.IO
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Path : IEquatable<Path>
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; }

        [JsonProperty(Required = Required.Default)]
        public bool CaseSensitive { get; }

        [JsonIgnore]
        public Uri Uri { get; }

        // TODO: should directories only apply when uri is a local file (uri.IsFile)?
        [JsonIgnore]
        public bool IsDirectory
        {
            get
            {
                if (Uri.IsAbsoluteUri)
                {
                    return Uri.AbsolutePath.EndsWith("/") || Uri.AbsolutePath.EndsWith("\\");
                }
                return Name.EndsWith("/") || Name.EndsWith("\\");
            }
        }

        [JsonIgnore]
        public bool IsAbsolute { get; }

        public Path(string name, bool caseSensitive = false)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CaseSensitive = caseSensitive;

            if (!Uri.TryCreate(name, UriKind.RelativeOrAbsolute, out Uri uri))
                throw new NotSupportedException("Invalid path.");

            // TODO: tighten up what constitutes as valid; for now, don't be a dick
            if (uri.IsAbsoluteUri)
            {
                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeFtp)
                {
          
                }
                else if (uri.Scheme == Uri.UriSchemeFile)
                {
                    // normalize name
                    Name = uri.LocalPath;
                }
                else throw new NotSupportedException();
            }
            else
            {
                // manual validation of relative paths
            }

            Uri = uri;
            IsAbsolute = uri.IsAbsoluteUri;
        }

        #region Casting

        /// <summary>
        /// Implicitly attempts to convert a string to a file path.
        /// </summary>
        /// <param name="path"></param>
        public static implicit operator Path (string path)
        {
            return new Path(path);
        }

        /// <summary>
        /// Implicitly converts a file path to a string.
        /// </summary>
        /// <param name="path"></param>
        public static implicit operator string (Path path)
        {
            return path.Name;
        }

        #endregion

        public ReadOnlyStream OpenFile()
        {
            throw new NotImplementedException();
        }

        // TODO: assert the path specified is a file?
        public static Path FromLocalFile(string localFilePath)
        {
            // TODO: make sure it doesn't end with trailing slash

            throw new NotImplementedException();
        }

        // TODO: assert the path specified is a directory?
        public static Path FromLocalDirectory(string localDirectoryPath)
        {
            // TODO: make sure it ends with trailing slash

            throw new NotImplementedException();
        }

        // TODO: cleanup
        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="first">This is assumed to be a directory path.</param>
        /// <param name="second">This can be a relative file or directory path.</param>
        /// <returns></returns>
        public static Path Combine(Path first, Path second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            if (second == null)
                throw new ArgumentNullException(nameof(second));

            if (second.IsAbsolute)
                throw new NotSupportedException("The second path must be relative.");

            if (first.Uri.Scheme == Uri.UriSchemeFile)
            {
                return new Path(System.IO.Path.Combine(first, second), first.CaseSensitive);
            }

            // TODO: cleanup
            string path = first.Name;
            if (first.Uri.Scheme == Uri.UriSchemeHttp || first.Uri.Scheme == Uri.UriSchemeHttps ||
                     first.Uri.Scheme == Uri.UriSchemeFtp)
            {
                if (!path.EndsWith("\\") && !path.EndsWith("/"))
                {
                    path += "/";
                }

                return new Path(path + second.Name.TrimStart('\\', '/'), first.CaseSensitive);
            }

            throw new NotSupportedException();
        }

        // TODO: normalize - collapse multiple separators into one, use same separator depending on URI type

        public static Path GetDirectoryName(Path path)
        {
            int index = path.Name.LastIndexOfAny(new[] {'\\', '/'});

            if (index != -1)
                return path.Name.Substring(0, index + 1);

            return null;
        }

        public static Path GetFileName(Path path)
        {
            int index = path.Name.LastIndexOfAny(new[] { '\\', '/' });

            if (index == -1)
                return path.Name;

            if (path.Name.Length > index + 1)
                return path.Name.Substring(index + 1);

            return null;
        }

        public static Path operator +(Path first, Path second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            if (second == null)
                throw new ArgumentNullException(nameof(second));

            return Combine(first, second);
        }

        public static Path operator +(string first, Path second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            if (second == null)
                throw new ArgumentNullException(nameof(second));

            return Combine(first, second);
        }

        public static Path operator +(Path first, string second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            if (second == null)
                throw new ArgumentNullException(nameof(second));

            return Combine(first, second);
        }

        #region Equality

        public bool Equals(Path other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name, CaseSensitive || other.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(string other)
        {
            if (ReferenceEquals(null, other)) return false;
            return string.Equals(Name, other, CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Path) obj);
        }

        #endregion

        public override int GetHashCode()
        {
            if (Name == null)
                return 0;

            return CaseSensitive ? Name.GetHashCode() : Name.ToUpperInvariant().GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
