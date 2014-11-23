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

        protected override void ProcessRecord()
        {
            try
            {
                var endpoint = GetBucketEndPoint();
                if (ListContents)
                {
                    var t = GetBucketContents(endpoint);
                    var contents = t.Result;
                    bool verbose = this.MyInvocation.BoundParameters.ContainsKey("Verbose");
                    foreach (var item in contents.items)
                    {
                        if (verbose)
                        {
                            WriteObject(item, true);
                        }
                        else
                        {
                            WriteObject(item.id);
                        }
                    }
                }
                else
                {
                    var t = GetBucketMetaData(endpoint);
                    dynamic result = t.Result;
                    WriteObject(result);
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

        private async Task<dynamic> GetBucketContents(dynamic endpoint)
        {
            return await endpoint.o.get(GetCancellationToken());
        }

        private dynamic GetBucketEndPoint()
        {
            dynamic google = CreateClient();

            return google.storage.v1.b(Bucket);
        }

        private async Task<dynamic> GetBucketMetaData(dynamic endpoint)
        {
            return await endpoint.get(GetCancellationToken());
        }
    }
}
