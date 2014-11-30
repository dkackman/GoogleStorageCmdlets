using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Collections.Generic;

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

                var cancelToken = GetCancellationToken();
                var accessTask = GetAccessToken(cancelToken);
                var access_token = accessTask.Result;

                IEnumerable<dynamic> items = contents.items;
                using (var pipeline = new DownloadPipline()
                {
                    Destination = Destination,
                    Force = Force
                })
                {
                    pipeline.Start(items, cancelToken, access_token);

                    int count = items.Count();
                    int i = 0;
                    foreach (var item in pipeline.Output.GetConsumingEnumerable())
                    {
                        WriteVerbose(string.Format("({0} of {1}) - Exported {2} to {3}", ++i, count, item.Item1.name, item.Item2));
                    }

                    foreach (var tuple in pipeline.Errors)
                    {
                        WriteError(new ErrorRecord(tuple.Item2, tuple.Item2.Message, ErrorCategory.ReadError, tuple.Item1));
                    }
                }
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
