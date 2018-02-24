using System.IO;
using System.Reflection;

namespace DiffBlit.Core.Extensions
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourcePath"></param>
        /// <param name="localPath"></param>
        public static void ExtractEmbeddedResource(this Assembly assembly, string resourcePath, string localPath)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (FileStream fileStream = new FileStream(localPath, FileMode.Create))
            {
                for (int i = 0; i < stream?.Length; i++)
                {
                    fileStream.WriteByte((byte)stream.ReadByte());
                }
                fileStream.Close();
            }
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        public static Stream GetEmbeddedResourceStream(this Assembly assembly, string resourcePath)
        {
            return assembly.GetManifestResourceStream(resourcePath);
        }
    }
}
