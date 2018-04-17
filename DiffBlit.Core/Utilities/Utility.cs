using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Forms;
using DiffBlit.Core.Delta;
using DiffBlit.Core.Extensions;
using DiffBlit.Core.IO;
using ICSharpCode.SharpZipLib.BZip2;
using Microsoft.WindowsAPICodePack.Dialogs;
using Path = DiffBlit.Core.IO.Path;

namespace DiffBlit.Core.Utilities
{
    /// <summary>
    /// Miscellaneous utility methods.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Computes an SHA512 hash of the specified file optionally reporting progress along the way.
        /// </summary>
        /// <param name="filePath">The path of the file to hash.</param>
        /// <param name="progressHandler">The optional handler to report progress to. Returns bytes read as the userState.</param>
        /// <returns></returns>
        public static byte[] ComputeHash(string filePath, ProgressChangedEventHandler progressHandler = null)
        {
            byte[] buffer = new byte[1024 * 64];

            // TODO: support other URIs
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, buffer.Length))
            using (SHA512 hash = SHA512.Create())
            using (CryptoStream hashStream = new CryptoStream(file, hash, CryptoStreamMode.Read))
            {
                int bytesRead;
                while ((bytesRead = hashStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    progressHandler?.Invoke(null,
                        new ProgressChangedEventArgs((int) ((float) file.Position / file.Length * 100.0f), bytesRead));
                }
                return hash.Hash;
            }
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="origFile"></param>
        /// <param name="compressedFile"></param>
        public static void Compress(string origFile, string compressedFile)
        {
            // TODO: support other URIs
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
            // TODO: support other URIs
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
        /// <param name="sourcePath">The source path containing parts of a file.</param>
        /// <param name="outputFilePath">The combined file path.</param>
        public static void JoinFiles(Path sourcePath, Path outputFilePath)
        {
            if (!outputFilePath.Uri.IsFile)
                throw new ArgumentException("Output path must be a file.");

            // TODO: support other URIs
            // TODO: use ReadOnlyFile and refactor to use sourceBasePath
            using (FileStream fs = File.OpenWrite(outputFilePath))
            {
                foreach (var file in Directory.GetFiles(sourcePath))
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
            // TODO: support other URIs
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
            string dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString()) + "\\";
            Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>
        /// Returns a file path to be used temporarily. Does not create a file.
        /// </summary>
        /// <returns></returns>
        public static string GetTempFilePath()
        {
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
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

        /// <summary>
        /// Shows a proper folder browser dialog and returns the selected directory path.
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public static string ShowDirectoryPicker(string prompt = null)
        {
            using (CommonOpenFileDialog ofd = new CommonOpenFileDialog())
            {
                ofd.IsFolderPicker = true;
                ofd.Title = prompt;
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    return ofd.FileName;
                }
            }
            return null;
        }

        /// <summary>
        /// Shows a proper folder browser dialog and returns the selected directory path.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="extensionFilter"></param>
        /// <returns></returns>
        public static string ShowFilePicker(string prompt = null, string extensionFilter = null)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = prompt;
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                ofd.Filter = extensionFilter;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    return ofd.FileName;
                }
            }
            return null;
        }

        /// <summary>
        /// Opens a read only stream to the given web Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static ReadOnlyStream OpenWebStream(Uri uri)
        {
            using (GZipWebClient client = new GZipWebClient())
            {
                return new ReadOnlyStream(client.OpenRead(uri));
            }
        }

        /// <summary>
        /// Downloads a file from the web to the specified path.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="path"></param>
        public static void DownloadWebFile(Uri uri, string path)
        {
            using (WebClient wc = new GZipWebClient())
            {
                wc.DownloadFile(uri, path);
            }
        }
    }
}
