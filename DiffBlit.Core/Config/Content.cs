using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiffBlit.Core.Extensions;
using DiffBlit.Core.Utilities;

namespace DiffBlit.Core.Config
{
    public class Content : List<FileInformation>, IEquatable<Content>
    {
        // cacheable in CloudFlare's free tier
        public const int DefaultChunkSize = 1024 * 1024 * 512;
        public const string DefaultChunkExtension = ".jar";

        public static Content Create(string inputFile, string outputDirectory, int chunkSize = DefaultChunkSize, bool compress = false, string fileExtension = DefaultChunkExtension)
        {
            Guid fileId = Guid.NewGuid();

            if (!File.Exists(inputFile))
                throw new FileNotFoundException("Unable to create content.", inputFile);
            
            if (!Directory.Exists(outputDirectory))
                throw new DirectoryNotFoundException("Unable to create content.");

            Content content = new Content();

            try
            {
                if (compress)
                {
                    string origPath = inputFile;
                    inputFile = Path.GetTempFileName();
                    Utility.Compress(origPath, inputFile);
                }

                using (FileStream fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    long size = fs.Length;
                    int iterations = (int)(size / chunkSize);
                    int remainder = (int)(size % chunkSize);

                    for (int i = 0; i < iterations; i++)
                    {
                        string path = Path.Combine(outputDirectory, $"{fileId}_{i}{fileExtension}");

                        using (var file = File.OpenWrite(path))
                        {
                            fs.CopyToCount(file, chunkSize);
                        }
                        var hash = Utility.ComputeHash(path);
                        content.Add(new FileInformation(path, hash));
                    }

                    if (remainder > 0)
                    {
                        string path = Path.Combine(outputDirectory, $"{fileId}_{iterations}{fileExtension}");

                        using (var last = File.OpenWrite(path))
                        {
                            fs.CopyToCount(last, remainder);
                        }
                        var hash = Utility.ComputeHash(path);
                        content.Add(new FileInformation(path, hash));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                if (compress)
                {
                    File.Delete(inputFile);
                }
            }

            return content;
        }

        /// <summary>
        /// Combines and saves content files into a single file.
        /// </summary>
        /// <param name="sourceDirectory">The directory which contains the content file(s).</param>
        /// <param name="outputFilePath">The path of the file to be created.</param>
        public void Save(string sourceDirectory, string outputFilePath)
        {
            switch (Count)
            {
                case 0:
                    throw new InvalidOperationException();
                case 1:
                    File.Copy(Path.Combine(sourceDirectory, this.First().Path), outputFilePath);
                    break;
                default:
                    using (FileStream fs = File.OpenWrite(outputFilePath))
                    {
                        foreach (var file in this)
                        {
                            using (FileStream s = File.OpenRead(Path.Combine(sourceDirectory, file.Path)))
                            {
                                s.CopyTo(fs);
                            }
                        }
                    }
                    break;
            }
        }

        public bool Equals(Content other)
        {
            if (Count != other?.Count)
                return false;

            for (int i = 0; i < Count; i++)
            {
                if (!this[i].Equals(other[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Content) obj);
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var item in this)
            {
                hash ^= item.GetHashCode();
            }
            return hash;
        }
    }
}
