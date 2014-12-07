using System;
using System.Diagnostics;
using System.Management.Automation;

namespace GoogleStorage.Buckets
{
    [Cmdlet(VerbsCommon.Add, "GoogleStorageBucket")]
    public class AddGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = false)]
        public string Project { get; set; }

        [Parameter(Mandatory = true, Position = 0)]
        public string Bucket { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var project = GetProjectName(Project);
                Debug.Assert(!string.IsNullOrEmpty(project));

                var api = CreateApiWrapper(project);
                var t = api.AddBucket(Bucket);

                dynamic result = t.Result;
                WriteVerbose(string.Format("Bucket {0} added to project {1}", Bucket, project));
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
