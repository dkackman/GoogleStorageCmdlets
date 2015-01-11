using System;
using System.Management.Automation;

namespace GoogleStorage.Buckets
{
    [Cmdlet(VerbsCommon.Remove, "GoogleStorageBucket", SupportsShouldProcess = true)]
    public class RemoveGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// The id of the bucket to remove
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Bucket { get; set; }

        /// <summary>
        /// Flag indicating whetehr to remove bucket without prompting
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess(Bucket, "Remove"))
                {
                    var msg = string.Format("Do you want to remove the bucket {0}?", Bucket);
                    if (Force || ShouldContinue(msg, "Remove bucket?"))
                    {
                        using (var api = CreateApiWrapper())
                        {
                            var t = api.RemoveBucket(Bucket);
                            t.Wait(api.CancellationToken);
                            WriteVerbose(string.Format("Bucket {0} removed", Bucket));
                        }
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
