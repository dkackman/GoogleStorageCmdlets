using System;
using System.Management.Automation;

namespace GoogleStorage.Buckets
{
    /// <summary>
    /// Indicates whether a given Bucket exists or not
    /// </summary>
    [Cmdlet(VerbsCommon.Find, "GoogleStorageBucket")]
    public class FindGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// The name of the Bucket to find
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                using (var api = CreateApiWrapper())
                {
                    var exists = api.FindBucket(Bucket).WaitForResult(GetCancellationToken());

                    WriteObject(exists);
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }
    }
}
