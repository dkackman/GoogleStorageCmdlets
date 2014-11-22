using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

using DynamicRestProxy.PortableHttpClient;

namespace GoogleStorage
{
    [Cmdlet(VerbsCommon.Get, "GoogleStorageBucket")]
    public class GetGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
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
                    foreach (var item in contents.items)
                    {
                        WriteObject(item);
                        Host.UI.WriteLine("");
                    }
                }
                else
                {
                    var t = GetBucketMetaData(endpoint);
                    dynamic result = t.Result;
                    WriteObject(result);
                }
            }
            catch (AggregateException e)
            {
                foreach (var error in e.InnerExceptions)
                {
                    WriteError(new ErrorRecord(error, error.Message, ErrorCategory.NotSpecified, null));
                }
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
