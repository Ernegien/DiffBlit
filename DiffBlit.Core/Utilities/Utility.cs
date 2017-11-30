using System.IO;
using System.Security.Cryptography;
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

        public static void Compress(string origFile, string compressedFile)
        {
            using (FileStream input = File.OpenRead(origFile))
            using (FileStream output = File.OpenWrite(compressedFile))
            using (BZip2OutputStream zip = new BZip2OutputStream(output))
            {
                input.CopyTo(zip);
            }
        }

        public static void Decompress(string compressedFile, string decompressedFile)
        {
            using (FileStream input = File.OpenRead(compressedFile))
            using (BZip2InputStream zip = new BZip2InputStream(input))
            using (FileStream output = File.OpenWrite(decompressedFile))
            {
                zip.CopyTo(output);
            }
        }
    }
}
