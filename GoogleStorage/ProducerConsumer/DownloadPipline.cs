using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics;

namespace GoogleStorage.ProducerConsumer
{
    class DownloadPipline
    {
        private BlockingCollection<dynamic>[] _objects = new BlockingCollection<dynamic>[4];

        public BlockingCollection<Tuple<dynamic, string>> Output { get; private set; }

        public string Destination { get; set; }

        public bool Force { get; set; }

        public DownloadPipline()
        {
            for (int i = 0; i < _objects.Length; i++)
            {
                _objects[i] = new BlockingCollection<dynamic>(1);
            }

            Output = new BlockingCollection<Tuple<dynamic, string>>();
        }

        public void Start(IEnumerable<dynamic> items, CancellationToken cancelToken, string access_token)
        {
            // this task will enumerate the items putting them into the blocking collection where they wait to be retreived by
            // another thread and downloaded - the _object array is esstenitally the throttle
            Task.Run(() =>
                {
                    foreach (var item in items)
                    {
                        BlockingCollection<dynamic>.AddToAny(_objects, item, cancelToken);
                    }

                    foreach (var collection in _objects)
                    {
                        collection.CompleteAdding();
                    }

                    Debug.WriteLine("done enumerated objects");
                });

            Action download = () =>
                {
                    dynamic item = null;

                    try
                    {
                        while (BlockingCollection<dynamic>.TakeFromAny(_objects, out item, cancelToken) > -1)
                        {
                            Task<Tuple<dynamic, string>> exportTask = ExportObject(item, cancelToken, access_token);
                            Output.Add(exportTask.Result);

                            item = null;
                        }
                    }
                    catch (ArgumentException)
                    {

                    }
                };

            // these are the download threads - each download blocks while in progress but the others can work in parallel
            Task.Run(() =>
                {
                    Task[] tasks = new Task[4];
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        tasks[i] = Task.Run(download);
                    }

                    Task.WaitAll(tasks);
                    Output.CompleteAdding();
                });
        }

        private async Task<Tuple<dynamic, string>> ExportObject(dynamic item, CancellationToken cancelToken, string access_token)
        {
            var path = Path.Combine(Destination, item.name);
            if (!Force && File.Exists(path))
            {
                throw new InvalidOperationException(string.Format("The file {0} already exists. Use -Force to overwrite existing files", path));
            }

            var downloader = new FileDownloader(item.mediaLink, path, item.contentType);

            await downloader.Download(cancelToken, access_token);

            return new Tuple<dynamic, string>(item, path);
        }
    }
}
