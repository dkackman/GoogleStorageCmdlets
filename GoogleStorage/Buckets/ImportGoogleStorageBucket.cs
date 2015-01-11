using System;
using System.IO;
using System.Threading.Tasks;
using System.Management.Automation;

namespace GoogleStorage.Buckets
{
    [Cmdlet(VerbsData.Import, "GoogleStorageBucket", SupportsShouldProcess = true)]
    public class ImportGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// Full path to the folder where objects to be imported exist.
        /// All objects in this folder will be included
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Source { get; set; }

        /// <summary>
        /// The id of the bucket into which objects will be imported
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        /// <summary>
        /// Semi-colon separated list of file masks of files to include while importing
        /// Defaults to *.*
        /// </summary>
        [Parameter(Mandatory = false, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string FileMasks { get; set; }

        /// <summary>
        /// Flag indicating wwhether to overwirte remote files without promting if they already exist
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Flag indicating whether to recurse sub folders and their contents under Source
        /// while importing
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Recurse { get; set; }

        public ImportGoogleStorageBucket()
        {
            FileMasks = "*.*";
        }

        protected override void ProcessRecord()
        {
            try
            {
                using(var api = CreateApiWrapper())
                using (var uploadPipeline = new Stage<Tuple<FileInfo, string>, Tuple<FileInfo, dynamic>>())
                {
                    // this is the delgate that does the uploading
                    Func<Tuple<FileInfo, string>, Task<Tuple<FileInfo, dynamic>>> func = async (input) =>
                    {
                        dynamic result = await api.ImportObject(input.Item1, input.Item2);

                        return new Tuple<FileInfo, dynamic>(input.Item1, result);
                    };

                    // this kicks off a number of async tasks that will do the uploads
                    // as items are added to the Input queue
                    uploadPipeline.Start(func, api.CancellationToken);

                    bool yesToAll = false;
                    bool noToAll = false;

                    int count = 0;
                    var files = new FileEnumerator(Source, FileMasks, Recurse);
                    if (ShouldProcess(Source, "import"))
                    {
                        foreach (var file in files.GetFiles())
                        {
                            var name = file.Name;

                            bool process = true;
                            // check yesToAll so we don't check the remote file if the user has already indicated they don't care
                            bool exists = api.FindObject(Bucket, name).WaitForResult(GetCancellationToken());
                            if (!yesToAll && !Force && exists)
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

                    // those tasks populate this blocking collection
                    // it will block until all of the tasks are complete 
                    // at which point we know the background threads are done and the enumeration will complete
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
