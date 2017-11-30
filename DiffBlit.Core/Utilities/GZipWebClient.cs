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
            WebRequest request = base.GetWebRequest(address);

            if (request is HttpWebRequest req)
            {
                req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            return request;
        }
    }
}
