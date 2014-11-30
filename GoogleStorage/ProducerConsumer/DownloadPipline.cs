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
    class DownloadPipline :IDisposable
    {
        private BlockingCollection<dynamic> _objects = new BlockingCollection<dynamic>(10);

        public ConcurrentBag<Tuple<dynamic, Exception>> Errors { get; private set; }

        public BlockingCollection<Tuple<dynamic, string>> Output { get; private set; }

        public string Destination { get; set; }

        public bool Force { get; set; }

        public int ThreadCount { get; set; }

        public DownloadPipline()
        {
            ThreadCount = 5;
            Output = new BlockingCollection<Tuple<dynamic, string>>();
            Errors = new ConcurrentBag<Tuple<dynamic, Exception>>();
        }

        public void Start(IEnumerable<dynamic> items, CancellationToken cancelToken, string access_token)
        {
            // this task will enumerate the items putting them into the blocking collection where they wait to be retreived by
            // another thread and downloaded - the _object blocking limit of this collection essentially is the throttle
            Task.Run(() =>
                {
                    foreach (var item in items)
                    {
                        _objects.Add(item, cancelToken);
                    }

                    _objects.CompleteAdding();
                });

            // this is teh delgate that does the downloading
            Action download = () =>
                {
                    foreach (var item in _objects.GetConsumingEnumerable())
                    {
                        try
                        {
                            Task<Tuple<dynamic, string>> exportTask = ExportObject(item, cancelToken, access_token);
                            Output.Add(exportTask.Result);
                        }
                        catch (Exception e)
                        {
                            Errors.Add(Tuple.Create(item, e));
                        }
                    }
                };

            // these are the download threads - each download blocks while in progress but the others can work in parallel
            Task.Run(() =>
                {
                    Task[] tasks = new Task[ThreadCount];
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        tasks[i] = Task.Run(download);
                    }

                    Task.WaitAll(tasks);
                    Output.CompleteAdding(); // this signals the calling thread that work is done
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

        public void Dispose()
        {
            _objects.Dispose();
            Output.Dispose();
        }
    }
}
