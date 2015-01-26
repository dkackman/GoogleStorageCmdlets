using System;
using System.IO;
using System.Management.Automation;

namespace GoogleStorage.Objects
{
    /// <summary>
    /// Downloads an object from google storage
    /// </summary>
    [Cmdlet(VerbsData.Export, "GoogleStorageObject", SupportsShouldProcess = true)]
    public class ExportGoogleStorageObject : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// The bucket where the object exists
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        /// <summary>
        /// The name of the object
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string ObjectName { get; set; }

        /// <summary>
        /// The full path to a folder where the object will be exported
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string Destination { get; set; }

        /// <summary>
        /// Flag indicating whether to save the meta data of the remote object along witht the file.
        /// Meta data is persisted in a file with the name "{remote_object_name}.metadata.json"
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter IncludeMetaData { get; set; }

        /// <summary>
        /// Flag inidcating to not prompt the before overwriting the local file if it exists
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                using (var api = CreateApiWrapper())
                {
                    var item = api.GetObject(Bucket, ObjectName).WaitForResult(CancellationToken);                    

                    if (ShouldProcess(string.Format("{0}/{1}", Bucket, ObjectName), "export"))
                    {
                        string path = Path.Combine(Destination, item.name).Replace('/', Path.DirectorySeparatorChar);

                        bool process = true;
                        if (File.Exists(path)) // if the file exists confirm the overwrite
                        {
                            var msg = string.Format("Do you want to overwrite the file {0}?", path);
                            process = Force || ShouldContinue(msg, "Overwrite file?");
                        }

                        if (process)
                        {
                            api.ExportObject(new Tuple<dynamic, string>(item, path), IncludeMetaData).Wait(CancellationToken);
                            WriteObject(path);
                            WriteVerbose(string.Format("Exported {0} to {1}", item.name, path));
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