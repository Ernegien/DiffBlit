using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using DiffBlit.Core.Config;
using DiffBlit.Core.Delta;
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
            // TODO: if source and destination are same, work against temp location
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
            // TODO: if source and destination are same, work against temp location
            using (FileStream input = File.OpenRead(compressedFile))
            using (BZip2InputStream zip = new BZip2InputStream(input))
            using (FileStream output = File.OpenWrite(decompressedFile))
            {
                zip.CopyTo(output);
            }
        }

        /// <summary>
        /// Combines files from the specified source directory into the specified ouput file. Assumes alphabetical sort indicates assembly order.
        /// </summary>
        /// <param name="sourceDirectory">The source directory containing parts of a file.</param>
        /// <param name="outputFilePath">The combined file path.</param>
        public static void JoinFiles(string sourceDirectory, string outputFilePath)
        {
            using (FileStream fs = File.OpenWrite(outputFilePath))
            {
                foreach (var file in Directory.GetFiles(sourceDirectory))
                {
                    using (FileStream s = File.OpenRead(file))
                    {
                        s.CopyTo(fs);
                    }
                }
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
        /// Creates and returns a directory path to be used temporarily.
        /// </summary>
        /// <returns></returns>
        public static string GetTempDirectory()
        {
            string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>
        /// Returns a file path to be used temporarily. Does not create a file.
        /// </summary>
        /// <returns></returns>
        public static string GetTempFilePath()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Gets a list of empty directory paths under the specified root path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> GetEmptyDirectories(string path)
        {
            return (from directory in Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
                    where !Directory.EnumerateFileSystemEntries(directory).Any() select directory).ToList();
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        public static IPatcher GetPatcher(PatchAlgorithmType algorithm)
        {
            IPatcher patcher;
            switch (algorithm)
            {
                case PatchAlgorithmType.BsDiff:
                    patcher = new BsDiffPatcher();
                    break;
                case PatchAlgorithmType.Fossil:
                    patcher = new FossilPatcher();
                    break;
                case PatchAlgorithmType.MsDelta:
                    patcher = new MsDeltaPatcher();
                    break;
                case PatchAlgorithmType.Octodiff:
                    patcher = new OctodiffPatcher();
                    break;
                case PatchAlgorithmType.PatchApi:
                    patcher = new PatchApiPatcher();
                    break;
                case PatchAlgorithmType.XDelta:
                    patcher = new XDeltaPatcher();
                    break;
                default:
                    throw new NotSupportedException("Invalid patch algorithm.");
            }
            return patcher;
        }
    }
}
