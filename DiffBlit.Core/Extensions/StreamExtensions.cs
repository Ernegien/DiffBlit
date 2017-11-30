using System;
using System.IO;

namespace DiffBlit.Core.Extensions
{
    public static class StreamExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(this Stream stream)
        {
            if (stream is MemoryStream)
            {
                return ((MemoryStream)stream).ToArray();
            }

            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Copies stream data of specified count to the destination stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="count"></param>
        public static void CopyTo(this Stream source, Stream destination, long count)
        {
            long bytesCopied = 0;
            byte[] buffer = new byte[0x1000];

            do
            {
                int bytesToRead = Math.Min((int)(count - bytesCopied), buffer.Length);
                int bytesRead = source.Read(buffer, 0, bytesToRead);
                destination.Write(buffer, 0, bytesRead);
                bytesCopied += bytesRead;
            } while (bytesCopied < count);
        }
    }
}
