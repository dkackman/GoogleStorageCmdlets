using System;
using System.Threading.Tasks;
using System.Management.Automation;

namespace GoogleStorage.Buckets
{
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

        /// <summary>
        /// The property name of each bucket to display. If not set Bucket name is displayed.
        /// Ignored if Verbose flag is set
        /// </summary>
        [Parameter(Mandatory = false)]
        public string DisplayProperty { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                using (var api = CreateApiWrapper())
                {

                    if (ListContents)
                    {
                        var contents = api.GetBucketContents(Bucket).WaitForResult(GetCancellationToken());
                        bool verbose = this.MyInvocation.BoundParameters.ContainsKey("Verbose");
                        foreach (var item in contents.items)
                        {
                            if (verbose)
                            {
                                WriteDynamicObject(item, DisplayProperty);
                                WriteVerbose("");
                            }
                            else
                            {
                                WriteObject(item.name);
                            }
                        }
                    }
                    else
                    {
                        dynamic result = api.GetBucket(Bucket).WaitForResult(GetCancellationToken());
                        WriteDynamicObject(result, DisplayProperty);
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
