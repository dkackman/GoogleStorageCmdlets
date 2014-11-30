using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Collections.Generic;
using System.Diagnostics;

using Newtonsoft.Json;

using GoogleStorage.ProducerConsumer;

namespace GoogleStorage.Buckets
{
    [Cmdlet(VerbsData.Export, "GoogleStorageBucket")]
    public class ExportGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string Destination { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter IncludeMetaData { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter BreakOnError { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var t = GetBucketContents();
                var contents = t.Result;
                IEnumerable<dynamic> items = contents.items;
                using (var objects = new Stage<dynamic, dynamic>(() => contents.items))
                using (var downloads = new Stage<dynamic, Tuple<dynamic, string>>(() => objects.Output.GetConsumingEnumerable(), d =>
                    {
                        Task<Tuple<dynamic, string>> task = ExportObject(d);
                        task.Wait();
                        return task.Result;
                    }))
                {
                    var tasks = new Task[] 
                    {
                        Task.Run(() => objects.Start()),
                        Task.Run(() => downloads.Start()),
                    };

                    int count = items.Count();
                    int i = 0;
                    foreach (var item in downloads.Output.GetConsumingEnumerable())
                    {
                        WriteVerbose(string.Format("({0} of {1}) - Exported {2} to {3}", ++i, count, item.Item1.name, item.Item2));
                    }
                }
                // ExportObjects(contents.items);
            }
            catch (HaltCommandException)
            {
            }
            catch (PipelineStoppedException)
            {
            }
            catch (AggregateException e)
            {
                WriteAggregateException(e);
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, e.Message, ErrorCategory.ReadError, null));
            }
        }


        private async Task<Tuple<dynamic, string>> ExportObject(dynamic item)
        {
            var path = Path.Combine(Destination, item.name);
            if (!Force && File.Exists(path))
            {
                throw new InvalidOperationException(string.Format("The file {0} already exists. Use -Force to overwrite existing files", path));
            }

            var downloader = new FileDownloader(item.mediaLink, path, item.contentType);
            var cancelToken = GetCancellationToken();
            var access_token = await GetAccessToken(cancelToken);
            await downloader.Download(cancelToken, access_token);

            return new Tuple<dynamic, string>(item, path);
        }

        private void SaveMetaData(dynamic item)
        {
            // build out the folder strucutre that might be embedded in the item name
            Directory.CreateDirectory(Path.Combine(Destination, Path.GetDirectoryName(item.name)));

            string path = Path.Combine(Destination, item.name + ".json");

            if (!Force && File.Exists(path))
            {
                WriteVerbose(string.Format("{0} exists. Skipping", path));
            }
            else
            {
                WriteVerbose(string.Format("Saving {0} metadata", item.name));

                using (var writer = new StreamWriter(path))
                {
                    string json = JsonConvert.SerializeObject(item);
                    writer.Write(json);
                }
            }
        }

        private async Task<dynamic> GetBucketContents()
        {
            dynamic google = CreateClient();

            return await google.storage.v1.b(Bucket).o.get(GetCancellationToken());
        }
    }
}
