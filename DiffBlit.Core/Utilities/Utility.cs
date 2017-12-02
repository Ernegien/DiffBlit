using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using DiffBlit.Core.Extensions;
using ICSharpCode.SharpZipLib.BZip2;

namespace DiffBlit.Core.Utilities
{
    /// <summary>
    /// Miscellaneous utility methods.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Computes an SHA512 hash of the specified file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static byte[] ComputeHash(string filePath)
        {
            using (FileStream file = File.OpenRead(filePath))
            using (SHA512 sha = SHA512.Create())
            {
                return sha.ComputeHash(file);
            }
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="origFile"></param>
        /// <param name="compressedFile"></param>
        public static void Compress(string origFile, string compressedFile)
        {
            using (FileStream input = File.OpenRead(origFile))
            using (FileStream output = File.OpenWrite(compressedFile))
            using (BZip2OutputStream zip = new BZip2OutputStream(output))
            {
                input.CopyTo(zip);
            }
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="compressedFile"></param>
        /// <param name="decompressedFile"></param>
        public static void Decompress(string compressedFile, string decompressedFile)
        {
            using (FileStream input = File.OpenRead(compressedFile))
            using (BZip2InputStream zip = new BZip2InputStream(input))
            using (FileStream output = File.OpenWrite(decompressedFile))
            {
                zip.CopyTo(output);
            }
        }

        /// <summary>
        /// Builds a dictionary of file paths and their respective content hashes in base64 encoding.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetFileHashes(string directory, bool recursive = true)
        {
            var fileHashes = new Dictionary<string, string>();
            var files = Directory.GetFiles(directory, "*", recursive ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
            foreach (var file in files)
            {
                fileHashes[file] = ComputeHash(file).ToBase64();
            }
            return fileHashes;
        }

        /// <summary>
        /// Returns a directory path to be used temporarily.
        /// </summary>
        /// <returns></returns>
        public static string GetTempDirectory()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Returns a file path to be used temporarily. Does not create a file.
        /// </summary>
        /// <returns></returns>
        public static string GetTempFilePath()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }
    }
}
