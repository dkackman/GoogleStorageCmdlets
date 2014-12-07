using System;
using System.Management.Automation;

namespace GoogleStorage.Objects
{
    [Cmdlet(VerbsCommon.Remove, "GoogleStorageObject", SupportsShouldProcess = true)]
    public class RemoveGoogleStorageObject : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string ObjectName { get; set; }

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
                    if (Force || ShouldContinue(msg, "Remove bucket?"))
                    {
                        var api = CreateApiWrapper();
                        var t = api.RemoveObject(Bucket, ObjectName);
                        t.Wait(api.CancellationToken);
                        WriteVerbose(string.Format("{0} removed", path));
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
