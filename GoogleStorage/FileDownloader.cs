using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

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

        public void Download(CancellationToken cancelToken, string access_token)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Destination));
            var s = "https://www.googleapis.com/download/storage/v1/b/uspto-pair/o/applications%2F05900002.zip?generation=1370956749027000&alt=media";

            var uri = new Uri(s);
            WebClient c = new WebClient();
            //c.Headers.Add("Authorization", "token " + "dddd");
            c.DownloadFile(s, Destination);

            //using(var client = new WebClient())
            //{
            //    client.DownloadFile(Source, Destination);
            //}
        }
    }
}