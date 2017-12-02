using System;
using System.Net;

namespace DiffBlit.Core.Utilities
{
    /// <summary>
    /// A WebClient extension that automatically handles gzipped content.
    /// </summary>
    public class GZipWebClient : WebClient
    {
        /// <inheritdoc />
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);

            if (request is HttpWebRequest req)
            {
                req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            return request;
        }
    }
}
