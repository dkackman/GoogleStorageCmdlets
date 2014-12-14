﻿using System;
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
                    Func<Tuple<FileInfo, string>, Task<Tuple<FileInfo, dynamic>>> func = async (input) =>
                    {
                        dynamic result = await api.ImportObject(input.Item1, input.Item2);

                        return new Tuple<FileInfo, dynamic>(input.Item1, result);
                    };

                    // this kicks off a number of async tasks that will do the downloads
                    // as items are added to the Input queue
                    uploadPipeline.Start(func, api.CancellationToken);

                    bool yesToAll = false;
                    bool noToAll = false;

                    int count = 0;
                    var files = new FileEnumerator(Source, "*.*");
                    if (ShouldProcess(Source, "import"))
                    {
                        foreach (var file in files.GetFiles())
                        {
                            var name = file.Name;

                            bool process = true;
                            // check yesToAll so we don't check the remote file if the user has already indicated they don't care
                            if (!yesToAll && !Force && RemoteFileExists(api, name))
                            {
                                var msg = string.Format("Do you want to overwrite the file {0}?", name);
                                process = Force || ShouldContinue(msg, "Overwrite file?", ref yesToAll, ref noToAll);
                            }

                            if (process)
                            {
                                uploadPipeline.Input.Add(Tuple.Create(file, name), api.CancellationToken);
                                count++;
                            }
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

        private bool RemoteFileExists(GoogleStorageApi api, string name)
        {
            Task<bool> exists = api.FindObject(Bucket, name);
            return exists.Result;
        }
    }
}
