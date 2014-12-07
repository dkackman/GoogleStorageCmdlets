using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Collections.Generic;

namespace GoogleStorage.Buckets
{
    [Cmdlet(VerbsData.Import, "GoogleStorageBucket", SupportsShouldProcess = true)]
    public class ImportGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Source { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var api = CreateApiWrapper();
                var t = api.GetBucketContents(Bucket);

                using (var uploadPipeline = new Stage<Tuple<string, string>, Tuple<string, string>>())
                {
                    // this is the delgate that does the downloading
                    Func<Tuple<string, string>, Tuple<string, string>> func = (input) =>
                    {
                        Task task = api.ImportObject(input);
                        task.Wait(api.CancellationToken);
                        return input;
                    };

                    // this kicks off a number of async tasks that will do the downloads
                    // as items are added to the Input queue
                    uploadPipeline.Start(func, api.CancellationToken);

                    bool yesToAll = false;
                    bool noToAll = false;
                    //foreach (var item in items)
                    //{
                    //    string path = Path.Combine(Destination, item.name).Replace('/', Path.DirectorySeparatorChar);
                    //    var tuple = new Tuple<string, string>(item, path);
                    //    if (File.Exists(path)) // if the file exists confirm the overwrite
                    //    {
                    //        if (ShouldProcess(path, "overwrite"))
                    //        {
                    //            var msg = string.Format("Do you want to overwrite the file {0}?", path);
                    //            if (Force || ShouldContinue(msg, "Overwrite file?", ref yesToAll, ref noToAll))
                    //            {
                    //                downloadPipeline.Input.Add(tuple, api.CancellationToken);
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        downloadPipeline.Input.Add(tuple, api.CancellationToken);
                    //    }
                    //}

                    // all of the items are enumerated and queued for download
                    // let the pipeline stage know that it can exit that enumeration when empty
                    uploadPipeline.Input.CompleteAdding();

                    //int count = items.Count();
                    //int i = 0;

                    //// those tasks populate this blocking collection
                    //// it will block until all of the tasks are complete 
                    //// at which point we know the background threads are done and the enumeration will complete
                    //foreach (var item in uploadPipeline.Output.GetConsumingEnumerable(api.CancellationToken))
                    //{
                    //    WriteVerbose(string.Format("({0} of {1}) - Exported {2} to {3}", ++i, count, item.Item1.name, item.Item2));
                    //}

                    foreach (var tuple in uploadPipeline.Errors)
                    {
                        WriteError(new ErrorRecord(tuple.Item2, tuple.Item2.Message, ErrorCategory.ReadError, tuple.Item1.Item1));
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
    }
}
