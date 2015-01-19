using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;

namespace GoogleStorage
{
    sealed class FileDownloader : IDisposable
    {
        private readonly HttpClient _client;

        public FileDownloader(string userAgent, SecureString access_token)
        {
            _client = CreateHttpClient(userAgent, access_token);
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
            }
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

        private static HttpClient CreateHttpClient(string agent, SecureString access_token)
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            var client = new HttpClient(handler, true);

            if (handler.SupportsTransferEncodingChunked())
            {
                client.DefaultRequestHeaders.TransferEncodingChunked = true;
            }

            ProductInfoHeaderValue productHeader = null;
            if (!string.IsNullOrEmpty(agent) && ProductInfoHeaderValue.TryParse(agent, out productHeader))
            {
                client.DefaultRequestHeaders.UserAgent.Clear();
                client.DefaultRequestHeaders.UserAgent.Add(productHeader);
            }

            if (access_token != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", access_token.ToUnsecureString());
            }

            return client;
        }

        public async Task Download(string source, string destination, string contentType, CancellationToken cancelToken)
        {
            // build out the folder strucutre that might be embedded in the item name
            var directory = Path.GetDirectoryName(destination);
            Directory.CreateDirectory(directory);

            // the item is a folder - nothing to download
            if (destination.TrimEnd('\\') == directory)
            {
                return;
            }

            var uri = new Uri(source);
            uri.ForceCanonicalPathAndQuery();

            var response = await _client.GetAsync(uri, cancelToken);
            response.EnsureSuccessStatusCode();

            using (var stream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await response.Content.CopyToAsync(stream);
            }
        }
    }
}