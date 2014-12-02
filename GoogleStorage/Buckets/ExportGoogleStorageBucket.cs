using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Collections.Generic;

using GoogleStorage.ProducerConsumer;

namespace GoogleStorage.Buckets
{
    [Cmdlet(VerbsData.Export, "GoogleStorageBucket")]
    public class ExportGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Destination { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter IncludeMetaData { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var cancelToken = GetCancellationToken();
                var t = GetBucketContents(cancelToken);
                var contents = t.Result;

                var accessTask = GetAccessToken(cancelToken);
                var access_token = accessTask.Result;
                IEnumerable<dynamic> items = contents.items;

                using (var pipeline = new DownloadPipline()
                {
                    Destination = Destination,
                    Force = Force,
                    UserAgent = GoogleStorageCmdlet.UserAgent,
                    IncludeMetaData = IncludeMetaData
                })
                {
                    pipeline.Start(items, cancelToken, access_token);

                    int count = items.Count();
                    int i = 0;
                    foreach (var item in pipeline.Output.GetConsumingEnumerable(cancelToken))
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

        private async Task<dynamic> GetBucketContents(CancellationToken cancelToken)
        {
            dynamic google = CreateClient();

            return await google.storage.v1.b(Bucket).o.get(cancelToken);
        }
    }
}
