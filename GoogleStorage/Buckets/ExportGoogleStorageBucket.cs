using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Collections.Generic;

namespace GoogleStorage.Buckets
{
    /// <summary>
    /// Exports the contents of a Google Storage Buckets 
    /// </summary>
    [Cmdlet(VerbsData.Export, "GoogleStorageBucket", SupportsShouldProcess = true)]
    public class ExportGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// The id of the Bucket to export
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        /// <summary>
        /// The full path to the folder where the bucket will be exported.
        /// Remote folders will be reflected beneath this location
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Destination { get; set; }

        /// <summary>
        /// Flag indicating whether to save the meta data of each remote object along witht the file.
        /// Meta data is persisted in a file with the name "{remote_object_name}.metadata.json"
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter IncludeMetaData { get; set; }

        /// <summary>
        /// Flag indicating wwhether to overwirte local files without promting if they already exist
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                using (var api = CreateApiWrapper())
                {
                    var contents = api.GetBucketContents(Bucket).WaitForResult(GetCancellationToken());

                    IEnumerable<dynamic> items = contents.items;

                    using (var downloadPipeline = new Stage<Tuple<dynamic, string>, Tuple<dynamic, string>>())
                    {
                        // this is the delgate that does the downloading
                        Func<Tuple<dynamic, string>, Task<Tuple<dynamic, string>>> func = async (input) =>
                            {
                                await api.ExportObject(input, IncludeMetaData);
                                return input;
                            };

                        // this kicks off a number of async tasks that will do 
                        // the downloads as items are added to the Input queue
                        downloadPipeline.Start(func, api.CancellationToken);

                        if (ShouldProcess(Bucket, "export"))
                        {
                            bool yesToAll = false;
                            bool noToAll = false;

                            foreach (var item in items)
                            {
                                string path = Path.Combine(Destination, item.name).Replace('/', Path.DirectorySeparatorChar);

                                bool process = true;
                                if (File.Exists(path)) // if the file exists confirm the overwrite
                                {
                                    var msg = string.Format("Do you want to overwrite the file {0}?", path);
                                    process = Force || ShouldContinue(msg, "Overwrite file?", ref yesToAll, ref noToAll);
                                }

                                if (process)
                                {
                                    downloadPipeline.Input.Add(new Tuple<dynamic, string>(item, path), api.CancellationToken);
                                }
                            }
                        }

                        // all of the items are enumerated and queued for download
                        // let the pipeline stage know that it can exit that enumeration when empty
                        downloadPipeline.Input.CompleteAdding();

                        int count = items.Count();
                        int i = 0;

                        // the tasks above populate this blocking collection
                        // it will block until all of the tasks are complete 
                        // at which point we know the background threads are done and the enumeration will complete
                        foreach (var item in downloadPipeline.Output.GetConsumingEnumerable(api.CancellationToken))
                        {
                            WriteVerbose(string.Format("({0} of {1}) - Exported {2} to {3}", ++i, count, item.Item1.name, item.Item2));
                        }

                        foreach (var tuple in downloadPipeline.Errors)
                        {
                            WriteError(new ErrorRecord(tuple.Item2, tuple.Item2.Message, ErrorCategory.ReadError, tuple.Item1.Item1));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }
    }
}
