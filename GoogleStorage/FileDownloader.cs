using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public string MimeType { get; private set; }

        public FileDownloader(string source, string destination, string mimeType)
        {
            Source = source;
            Destination = destination;
            MimeType = mimeType;
        }

        public async Task Download(CancellationToken cancelToken, string access_token)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Destination));

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
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MimeType));

                if (!string.IsNullOrEmpty(access_token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", access_token);
                }

                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                using (var stream = new FileStream(Destination, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(stream);
                }
            }
        }
    }
}