using System;
using System.Net;

namespace DiffBlit.Core.Utilities
{
    /// <inheritdoc />
    public class GZipWebClient : WebClient
    {
        /// <inheritdoc />
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return request;
        }
    }
}
