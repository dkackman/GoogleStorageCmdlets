using System;
using System.Management.Automation;

namespace GoogleStorage.Objects
{
    /// <summary>
    /// Deletes a remote object from google storage
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "GoogleStorageObject", SupportsShouldProcess = true)]
    public class RemoveGoogleStorageObject : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// The bucket where the object exists
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        /// <summary>
        /// The name of the object to remove
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string ObjectName { get; set; }

        /// <summary>
        /// Flag indicating whther to remove the object without prompting the user
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                string path = string.Format("{0}/{1}", Bucket, ObjectName);
                if (ShouldProcess(path, "Remove"))
                {
                    var msg = string.Format("Do you want to remove the object {0}?", path);
                    if (Force || ShouldContinue(msg, "Remove object?"))
                    {
                        using (var api = CreateApiWrapper())
                        {
                            api.RemoveObject(Bucket, ObjectName).Wait(GetCancellationToken());
                        }
                        WriteVerbose(string.Format("{0} removed", path));
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
