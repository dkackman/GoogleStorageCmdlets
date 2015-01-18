using System;
using System.Threading.Tasks;
using System.Management.Automation;

namespace GoogleStorage.Buckets
{
    /// <summary>
    /// Retreives the properties or contents of a Bucket
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "GoogleStorageBucket")]
    public class GetGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// The id of the bucket
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        /// <summary>
        /// Flag indicating whether to display the contents of the bucket rather than show its properties
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter ListContents { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                using (var api = CreateApiWrapper())
                {

                    if (ListContents)
                    {
                        var contents = api.GetBucketContents(Bucket).WaitForResult(GetCancellationToken());
                        foreach (var item in contents.items)
                        {
                            WriteDynamicObject(item);
                        }
                    }
                    else
                    {
                        dynamic result = api.GetBucket(Bucket).WaitForResult(GetCancellationToken());
                        WriteDynamicObject(result);
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
