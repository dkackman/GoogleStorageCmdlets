using System;
using System.IO;
using System.Management.Automation;

namespace GoogleStorage.Objects
{
    [Cmdlet(VerbsData.Import, "GoogleStorageObject", SupportsShouldProcess = true)]
    public class ImportGoogleStorageObject : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Source { get; set; }

        [Parameter(Mandatory = false, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string ObjectName { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                using (var api = CreateApiWrapper())
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
