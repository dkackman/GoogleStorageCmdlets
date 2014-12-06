using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;

using Newtonsoft.Json;

namespace GoogleStorage.ProducerConsumer
{
    class Stage<TInput, TOutput> : IDisposable
    {
        public BlockingCollection<Tuple<dynamic, string>> Input { get; private set; }

        public BlockingCollection<Tuple<dynamic, string>> Output { get; private set; }

        public ConcurrentBag<Tuple<Tuple<dynamic, string>, Exception>> Errors { get; private set; }

        public int ThreadCount { get; set; }

        public string UserAgent { get; set; }

        public bool IncludeMetaData { get; set; }

        public Stage()
        {
            ThreadCount = 5;
            Input = new BlockingCollection<Tuple<dynamic, string>>();
            Output = new BlockingCollection<Tuple<dynamic, string>>();
            Errors = new ConcurrentBag<Tuple<Tuple<dynamic, string>, Exception>>();
        }

        public void Start(CancellationToken cancelToken, string access_token)
        {
            // this is the delegate that does the downloading
            Action download = () =>
                {
                    foreach (var item in Input.GetConsumingEnumerable(cancelToken))
                    {
                        try
                        {
                            var exportTask = ExportObject(item, cancelToken, access_token);
                            Output.Add(exportTask.Result);
                        }
                        catch (AggregateException e)
                        {
                            foreach (var ex in e.InnerExceptions)
                            {
                                Errors.Add(Tuple.Create(item, ex));
                            }
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
                        tasks[i] = Task.Run(download, cancelToken);
                    }

                    Task.WaitAll(tasks, cancelToken);
                    Output.CompleteAdding(); // this signals the calling thread that work is done
                }, cancelToken);
        }

        private async Task<Tuple<dynamic, string>> ExportObject(Tuple<dynamic, string> item, CancellationToken cancelToken, string access_token)
        {
            var downloader = new FileDownloader(item.Item1.mediaLink, item.Item2, item.Item1.contentType, UserAgent);

            await downloader.Download(cancelToken, access_token);

            if (IncludeMetaData)
            {
                SaveMetaData(item);
            }

            return item;
        }

        private void SaveMetaData(Tuple<dynamic, string> item)
        {
            using (var writer = new StreamWriter(item.Item2 + ".metadata.json"))
            {
                string json = JsonConvert.SerializeObject(item.Item1);
                writer.Write(json);
            }
        }

        public void Dispose()
        {
            Input.Dispose();
            Output.Dispose();
        }
    }
}
