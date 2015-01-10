using System;
using System.Threading.Tasks;
using System.Management.Automation;

namespace GoogleStorage.Buckets
{
    [Cmdlet(VerbsCommon.Get, "GoogleStorageBucket")]
    public class GetGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter ListContents { get; set; }

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
