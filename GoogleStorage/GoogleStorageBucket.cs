using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

using DynamicRestProxy.PortableHttpClient;

namespace GoogleStorage
{
    [Cmdlet(VerbsCommon.Show, "GoogleStorageBucket")]

    public class ShowGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var t = GetBucket();
                dynamic result = t.Result;
                WriteObject(result);
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

        private async Task<dynamic> GetBucket()
        {
            dynamic google = CreateClient();
            dynamic bucketEndPoint = google.storage.v1.b(Bucket);

            return await bucketEndPoint.get(GetCancellationToken());
        }
    }
}
