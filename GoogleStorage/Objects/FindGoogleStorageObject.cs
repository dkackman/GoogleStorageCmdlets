using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Collections.Generic;

namespace GoogleStorage.Objects
{
    [Cmdlet(VerbsCommon.Find, "GoogleStorageObject")]
    public class FindGoogleStorageObject : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string ObjectName { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var api = CreateApiWrapper();
                var t = api.FindObject(Bucket, ObjectName);

                WriteObject(t.Result);
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
