using System;
using System.IO;
using System.Management.Automation;

namespace GoogleStorage.Objects
{
    /// <summary>
    /// Uploads a file into a google storage bucket
    /// </summary>
    [Cmdlet(VerbsData.Import, "GoogleStorageObject", SupportsShouldProcess = true)]
    public class ImportGoogleStorageObject : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// The name of the target bucket
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        /// <summary>
        /// Full path to the file to upload
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Source { get; set; }

        /// <summary>
        /// Optional name of the remote object. The local file name is used if not supplied
        /// </summary>
        [Parameter(Mandatory = false, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string ObjectName { get; set; }

        /// <summary>
        /// Flag inidcating to not prompt the before overwriting the remote file if it exists
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                using (
                    var api = CreateApiWrapper())
                {
                    if (!api.FindBucket(Bucket).WaitForResult(GetCancellationToken()))
                    {
                        throw new ItemNotFoundException(string.Format("The bucket {0} does not exist. Call Add-GoogleStorageBucket first.", Bucket));
                    }

                    if (ShouldProcess(Source, "import"))
                    {
                        var file = new FileInfo(Source);
                        var name = string.IsNullOrEmpty(ObjectName) ? file.Name : ObjectName;

                        bool exists = api.FindObject(Bucket, name).WaitForResult(GetCancellationToken());

                        bool process = true;
                        if (!Force && exists)
                        {
                            var msg = string.Format("Do you want to overwrite the file {0} in bucket {1}?", name, Bucket);
                            process = Force || ShouldContinue(msg, "Overwrite remote file?");
                        }

                        if (process)
                        {
                            dynamic result = api.ImportObject(file, name, Bucket).WaitForResult(GetCancellationToken());
                            WriteDynamicObject(result);
                            WriteVerbose(string.Format("Imported {0} to {1}", Source, result.name));
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
