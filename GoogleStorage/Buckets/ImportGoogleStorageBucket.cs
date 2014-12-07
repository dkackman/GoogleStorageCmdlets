using System;
using System.IO;
using System.Threading.Tasks;
using System.Management.Automation;

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

                using (var uploadPipeline = new Stage<Tuple<FileInfo, string>, Tuple<FileInfo, dynamic>>())
                {
                    // this is the delgate that does the downloading
                    Func<Tuple<FileInfo, string>, Tuple<FileInfo, dynamic>> func = (input) =>
                    {
                        Task<dynamic> task = api.ImportObject(input.Item1, input.Item2);
                        task.Wait(api.CancellationToken);
                        return new Tuple<FileInfo, dynamic>(input.Item1,  task.Result);
                    };

                    // this kicks off a number of async tasks that will do the downloads
                    // as items are added to the Input queue
                    uploadPipeline.Start(func, api.CancellationToken);

                    bool yesToAll = false;
                    bool noToAll = false;

                    int count = 0;
                    var files = new FileEnumerator(Source, "*.*");
                    foreach (var file in files.GetFiles())
                    {
                        if (ShouldProcess(file.Name, "imort"))
                        {
                            uploadPipeline.Input.Add(Tuple.Create(file, file.Name), api.CancellationToken);
                            count++;
                        }
                    }

                    // all of the items are enumerated and queued for download
                    // let the pipeline stage know that it can exit that enumeration when empty
                    uploadPipeline.Input.CompleteAdding();

                    //// those tasks populate this blocking collection
                    //// it will block until all of the tasks are complete 
                    //// at which point we know the background threads are done and the enumeration will complete
                    int i = 0;
                    foreach (var item in uploadPipeline.Output.GetConsumingEnumerable(api.CancellationToken))
                    {
                        WriteVerbose(string.Format("({0} of {1}) - Imported {2} to {3}", ++i, count, item.Item1.Name, item.Item2.name));
                    }

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
