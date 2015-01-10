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

        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Source { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string ObjectName { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            using (var api = CreateApiWrapper())
            {
                bool exists = api.FindObject(Bucket, ObjectName).WaitForResult(GetCancellationToken());
            }
        }
    }
}
