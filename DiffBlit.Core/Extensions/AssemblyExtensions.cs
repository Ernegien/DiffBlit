using System.IO;
using System.Reflection;

namespace DiffBlit.Core.Extensions
{
    public static class AssemblyExtensions
    {
        public static void ExtractEmbeddedResource(this Assembly assembly, string outputDir, string resourceLocation, string file)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceLocation + @"." + file))
            using (FileStream fileStream = new FileStream(Path.Combine(outputDir, file), FileMode.Create))
            {
                for (int i = 0; i < stream?.Length; i++)
                {
                    fileStream.WriteByte((byte)stream.ReadByte());
                }
                fileStream.Close();
            }
        }

        public static Stream GetEmbeddedResourceStream(this Assembly assembly, string outputDir, string resourcePath)
        {
            return assembly.GetManifestResourceStream(resourcePath);
        }
    }
}
