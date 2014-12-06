using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GoogleStorage
{
    class FileDownloader
    {
        public string Source { get; private set; }

        public string Destination { get; private set; }

        public string ContentType { get; private set; }

        public string UserAgent { get; private set; }

        public FileDownloader(string source, string destination, string contentType, string userAgent)
        {
            Source = source;
            Destination = destination;
            ContentType = FormatContentType(contentType);
            UserAgent = userAgent;
        }

        private static string FormatContentType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return "";
            }

            var semi = contentType.IndexOf(';');
            if (semi > -1)
            {
                return contentType.Substring(0, semi);
            }

            return contentType;
        }

        public async Task Download(CancellationToken cancelToken, string access_token)
        {
            // build out the folder strucutre that might be embedded in the item name
            var directory = Path.GetDirectoryName(Destination);
            Directory.CreateDirectory(directory);

            // the item is a folder - nothing to download
            if (Destination.TrimEnd('\\') == directory)
            {
                return;
            }

            var uri = new Uri(Source);
            uri.ForceCanonicalPathAndQuery();

            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            using (var client = new HttpClient(handler, true))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType));

                ProductInfoHeaderValue productHeader = null;
                if (!string.IsNullOrEmpty(UserAgent) && ProductInfoHeaderValue.TryParse(UserAgent, out productHeader))
                {
                    client.DefaultRequestHeaders.UserAgent.Clear();
                    client.DefaultRequestHeaders.UserAgent.Add(productHeader);
                }

                if (handler.SupportsTransferEncodingChunked())
                {
                    client.DefaultRequestHeaders.TransferEncodingChunked = true;
                }

                if (!string.IsNullOrEmpty(access_token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", access_token);
                }

                var response = await client.GetAsync(uri, cancelToken);
                response.EnsureSuccessStatusCode();

                using (var stream = new FileStream(Destination, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(stream);
                }
            }
        }
    }
}