using System;
using System.Diagnostics;
using System.Management.Automation;

namespace GoogleStorage.Buckets
{
    [Cmdlet(VerbsCommon.Add, "GoogleStorageBucket")]
    public class AddGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// The Google Storage project name where the bucket will be added
        /// If not set uses <see cref="GoogleStorage.Config.SetGoogleStorageConfig.Project"/>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Project { get; set; }

        /// <summary>
        /// The name of the bucket to add
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Bucket { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var project = GetProjectName(Project);
                Debug.Assert(!string.IsNullOrEmpty(project));

                using (var api = CreateApiWrapper(project))
                {
                    dynamic result = api.AddBucket(Bucket).WaitForResult(GetCancellationToken());
                    WriteVerbose(string.Format("Bucket {0} added to project {1}", Bucket, project));
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
